Feature: Saga de Ordem de Servico
  Como tecnico da oficina
  Quero gerenciar o ciclo completo de uma Ordem de Servico
  Para garantir rastreabilidade e fluxo correto de aprovacao

  Scenario: Fluxo completo de aprovacao e execucao de OS
    Given uma Ordem de Servico e criada para o cliente com descricao "Troca de oleo"
    When o tecnico inicia o diagnostico
    And o tecnico registra o diagnostico "Motor com desgaste" com valor estimado de 1500.00
    And o pagamento e aprovado pelo sistema
    And a execucao e concluida com valor final de 1500.00
    And a entrega e registrada
    Then a Ordem de Servico deve ter status "Entregue"
    And o historico deve conter 6 entradas

  Scenario: Cancelamento de OS em diagnostico
    Given uma Ordem de Servico e criada para o cliente com descricao "Verificacao geral"
    When o tecnico inicia o diagnostico
    And a OS e cancelada pelo tecnico com motivo "Cliente desistiu"
    Then a Ordem de Servico deve ter status "Cancelada"

  Scenario: Tentativa de transicao invalida
    Given uma Ordem de Servico e criada para o cliente com descricao "Revisao"
    When uma transicao invalida e tentada
    Then uma excecao de operacao invalida deve ser lancada
