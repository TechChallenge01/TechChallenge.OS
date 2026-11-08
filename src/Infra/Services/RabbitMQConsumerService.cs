using System.Text;
using System.Text.Json;
using Application.Controllers.OrdensServicos;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Infra.Services;

public class RabbitMQConsumerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IChannel? _channel;
    private const string Exchange = "tech.saga";

    public RabbitMQConsumerService(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:Host"] ?? "localhost",
            Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
            UserName = _configuration["RabbitMQ:UserName"] ?? "guest",
            Password = _configuration["RabbitMQ:Password"] ?? "guest"
        };

        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await _channel.ExchangeDeclareAsync(Exchange, ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);

        await BindQueue("os.orcamento.aprovado", "orcamento.aprovado", stoppingToken);
        await BindQueue("os.orcamento.rejeitado", "orcamento.rejeitado", stoppingToken);
        await BindQueue("os.pagamento.confirmado", "pagamento.confirmado", stoppingToken);
        await BindQueue("os.pagamento.falhou", "pagamento.falhou", stoppingToken);
        await BindQueue("os.execucao.finalizada", "execucao.finalizada", stoppingToken);
        await BindQueue("os.execucao.falhou", "execucao.falhou", stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += HandleMessageAsync;

        await _channel.BasicConsumeAsync("os.orcamento.aprovado", autoAck: false, consumer, stoppingToken);
        await _channel.BasicConsumeAsync("os.orcamento.rejeitado", autoAck: false, consumer, stoppingToken);
        await _channel.BasicConsumeAsync("os.pagamento.confirmado", autoAck: false, consumer, stoppingToken);
        await _channel.BasicConsumeAsync("os.pagamento.falhou", autoAck: false, consumer, stoppingToken);
        await _channel.BasicConsumeAsync("os.execucao.finalizada", autoAck: false, consumer, stoppingToken);
        await _channel.BasicConsumeAsync("os.execucao.falhou", autoAck: false, consumer, stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task BindQueue(string queue, string routingKey, CancellationToken ct)
    {
        await _channel!.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false, cancellationToken: ct);
        await _channel.QueueBindAsync(queue, Exchange, routingKey, cancellationToken: ct);

        var dlq = $"{queue}.dlq";
        await _channel.QueueDeclareAsync(dlq, durable: true, exclusive: false, autoDelete: false, cancellationToken: ct);
    }

    private async Task HandleMessageAsync(object sender, BasicDeliverEventArgs ea)
    {
        var body = Encoding.UTF8.GetString(ea.Body.ToArray());
        var routingKey = ea.RoutingKey;

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dataSource = scope.ServiceProvider.GetRequiredService<IOrdemServicoDataSource>();
            var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
            var systemUserId = Guid.Parse(_configuration["SystemUserId"] ?? Guid.Empty.ToString());
            var controller = new OrdemServicoController(dataSource);

            switch (routingKey)
            {
                case "orcamento.aprovado":
                {
                    var payload = JsonSerializer.Deserialize<OrcamentoAprovado>(body)!;
                    await controller.AtualizarStatus(payload.OrdemServicoId, EStatusOS.EmExecucao, systemUserId, null, ct: default);
                    break;
                }
                case "orcamento.rejeitado":
                {
                    var payload = JsonSerializer.Deserialize<OrcamentoRejeitado>(body)!;
                    await controller.Cancelar(payload.OrdemServicoId,
                        new Shared.DTOs.OrdemServicos.Request.CancelarOrdemServicoRequestDTO { Motivo = payload.Motivo },
                        systemUserId, eventPublisher, ct: default);
                    break;
                }
                case "pagamento.confirmado":
                {
                    var payload = JsonSerializer.Deserialize<PagamentoConfirmado>(body)!;
                    await controller.AtualizarStatus(payload.OrdemServicoId, EStatusOS.EmExecucao, systemUserId, null, ct: default);
                    break;
                }
                case "pagamento.falhou":
                {
                    var payload = JsonSerializer.Deserialize<PagamentoFalhou>(body)!;
                    await controller.Cancelar(payload.OrdemServicoId,
                        new Shared.DTOs.OrdemServicos.Request.CancelarOrdemServicoRequestDTO { Motivo = payload.Motivo },
                        systemUserId, eventPublisher, ct: default);
                    break;
                }
                case "execucao.finalizada":
                {
                    var payload = JsonSerializer.Deserialize<ExecucaoFinalizada>(body)!;
                    await controller.Finalizar(payload.OrdemServicoId, payload.ValorFinal, systemUserId, eventPublisher, ct: default);
                    break;
                }
                case "execucao.falhou":
                {
                    var payload = JsonSerializer.Deserialize<ExecucaoFalhou>(body)!;
                    await controller.Cancelar(payload.OrdemServicoId,
                        new Shared.DTOs.OrdemServicos.Request.CancelarOrdemServicoRequestDTO { Motivo = payload.Motivo },
                        systemUserId, eventPublisher, ct: default);
                    break;
                }
            }

            await _channel!.BasicAckAsync(ea.DeliveryTag, false);
        }
        catch
        {
            await _channel!.BasicNackAsync(ea.DeliveryTag, false, false);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null) await _channel.DisposeAsync();
        if (_connection is not null) await _connection.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }

    private record OrcamentoAprovado(Guid OrdemServicoId, Guid PagamentoId, string LinkPagamento);
    private record OrcamentoRejeitado(Guid OrdemServicoId, string Motivo);
    private record PagamentoConfirmado(Guid OrdemServicoId);
    private record PagamentoFalhou(Guid OrdemServicoId, string Motivo);
    private record ExecucaoFinalizada(Guid OrdemServicoId, decimal ValorFinal);
    private record ExecucaoFalhou(Guid OrdemServicoId, string Motivo);
}
