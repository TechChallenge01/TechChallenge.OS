# TechChallenge.OS — Microsserviço de Ordens de Serviço

Microsserviço responsável pelo ciclo de vida completo de Ordens de Serviço (OS), parte do sistema TechChallenger (oficina mecânica).

## Responsabilidades

- Abertura de novas Ordens de Serviço
- Controle de status: Recebida → EmDiagnostico → AguardandoAprovacao → EmExecucao → Finalizada → Entregue
- Histórico de transições de status com rastreabilidade
- Publicação de eventos para coordenação da Saga
- Cancelamento com compensação automática

## Arquitetura

Clean Architecture com 5 camadas: **Domain**, **Shared**, **Application**, **Infra**, **API**.

## Banco de Dados

- **MongoDB** — banco de dados NoSQL
- Coleção: `ordens_servico`
- Histórico de status embutido por documento (Document Model)

## Padrão Saga — Coreografado via RabbitMQ

Este serviço é o **ponto de entrada** da Saga coreografada do fluxo de Ordem de Serviço.

### Por que Coreografia?

Escolhemos **Coreografia** (em vez de Orquestração) pelos seguintes motivos:
- **Desacoplamento total**: cada serviço conhece apenas os eventos que deve publicar/consumir, sem depender de um orquestrador central
- **Sem ponto único de falha**: não há um orquestrador que, se cair, trava todo o fluxo
- **Resiliência independente**: cada serviço pode ser escalado, deployado e mantido sem coordenação explícita
- **Simplicidade**: o fluxo é simples o suficiente para não exigir o overhead de um orquestrador

### Eventos publicados

| Exchange | Routing Key | Quando |
|----------|-------------|--------|
| `tech.saga` | `os.aberta` | Nova OS criada |
| `tech.saga` | `os.diagnostico.registrado` | Diagnóstico registrado com valor estimado |
| `tech.saga` | `os.cancelada` | OS cancelada (pagamento falhou ou solicitação do cliente) |
| `tech.saga` | `os.finalizada` | Reparo concluído com valor final |
| `tech.saga` | `os.entregue` | Veículo entregue ao cliente |

### Eventos consumidos

| Exchange | Routing Key | Ação |
|----------|-------------|------|
| `tech.saga` | `pagamento.confirmado` | Chama `IniciarExecucao` na OS |
| `tech.saga` | `pagamento.falhou` | Cancela a OS |
| `tech.saga` | `execucao.finalizada` | Finaliza a OS com valor final |

### Compensação (Rollback)

- `pagamento.falhou` → chama `os.Cancelar()`, publica `os.cancelada`
- `execucao.falhou` → mantém em `EmExecucao`, aguarda retry ou intervenção manual
- Dead Letter Queue (`os.saga.dlq`) captura mensagens não processadas

## Executando localmente

```bash
# Pré-requisitos: Docker, .NET 10 SDK
docker compose up -d
dotnet run --project src/API
```

## Variáveis de ambiente

| Variável | Descrição |
|----------|-----------|
| `ConnectionStrings__MongoDB` | URI do MongoDB |
| `MongoDB__DatabaseName` | Nome do banco (default: `techchallenge_os`) |
| `RabbitMQ__Host` | Host do RabbitMQ |
| `RabbitMQ__Username` | Usuário RabbitMQ |
| `RabbitMQ__Password` | Senha RabbitMQ |
| `Jwt__Key` | Chave JWT (mínimo 32 chars) |
| `Jwt__Issuer` | Issuer JWT (default: `TechChallenger`) |
| `Jwt__Audience` | Audience JWT (default: `TechChallenger`) |

## Endpoints principais

| Método | Rota | Descrição |
|--------|------|-----------|
| `POST` | `/ordens-servico` | Abrir nova OS |
| `GET` | `/ordens-servico` | Listar OSs (paginado) |
| `GET` | `/ordens-servico/{id}` | Buscar OS por ID |
| `POST` | `/ordens-servico/{id}/iniciar-diagnostico` | Iniciar diagnóstico |
| `POST` | `/ordens-servico/{id}/registrar-diagnostico` | Registrar diagnóstico + valor estimado |
| `PATCH` | `/ordens-servico/{id}/status` | Atualizar status |
| `POST` | `/ordens-servico/{id}/finalizar` | Finalizar com valor final |
| `DELETE` | `/ordens-servico/{id}` | Cancelar OS |

## Kubernetes

Manifestos em `k8s/`:
- `namespace.yaml` — namespace `techchallenge`
- `deployment.yaml` — 2 réplicas, liveness/readiness probes
- `service.yaml` — ClusterIP porta 80
- `hpa.yaml` — auto-scaling 2–5 pods (70% CPU)

Deploy via: `.github/workflows/k8s-deploy.yml` (manual, `workflow_dispatch`)

## CI/CD

| Workflow | Trigger | Descrição |
|----------|---------|-----------|
| `deploy.yml` | push main (src/) | Build, push ECR, deploy ECS |
| `infra.yml` | push main (infra/) | Terraform apply |
| `quality.yml` | push/PR main | Testes unitários + SonarCloud |
| `k8s-deploy.yml` | manual | Deploy para EKS |

## Testes

```bash
dotnet test tests/UnitTests
```

Cobertura mínima: **80%** — validada pelo SonarCloud (`.github/workflows/quality.yml`).

**Configuração SonarCloud** (GitHub → Settings → Secrets/Variables):
- Secret: `SONAR_TOKEN`
- Variable: `SONAR_PROJECT_KEY`, `SONAR_ORGANIZATION`
