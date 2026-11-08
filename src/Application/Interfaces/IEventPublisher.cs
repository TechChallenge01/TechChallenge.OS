namespace Application.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync<T>(string eventName, T payload, CancellationToken ct = default) where T : class;
}
