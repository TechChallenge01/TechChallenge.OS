# ─── IAM — AWS Academy usa a LabRole pré-existente ───────────────────────────
# No AWS Academy não é possível criar IAM users/roles arbitrários.
# A LabRole já tem PowerUserAccess + permissões para ECR, Secrets Manager e CloudWatch.

data "aws_iam_role" "lab_role" {
  name = "LabRole"
}

# ─── Cluster ──────────────────────────────────────────────────────────────────

resource "aws_ecs_cluster" "main" {
  name = var.project_name

  setting {
    name  = "containerInsights"
    value = "enabled"
  }
}

resource "aws_ecs_cluster_capacity_providers" "main" {
  cluster_name       = aws_ecs_cluster.main.name
  capacity_providers = ["FARGATE"]

  default_capacity_provider_strategy {
    base              = 1
    weight            = 100
    capacity_provider = "FARGATE"
  }
}

# ─── OS Service ───────────────────────────────────────────────────────────────

resource "aws_ecs_task_definition" "os_service" {
  family                   = "os-service"
  network_mode             = "awsvpc"
  requires_compatibilities = ["FARGATE"]
  cpu                      = "512"
  memory                   = "1024"
  execution_role_arn       = data.aws_iam_role.lab_role.arn
  task_role_arn            = data.aws_iam_role.lab_role.arn

  container_definitions = jsonencode([{
    name      = "os-service"
    image     = "${aws_ecr_repository.os_service.repository_url}:latest"
    essential = true

    portMappings = [{
      containerPort = 8080
      protocol      = "tcp"
    }]

    environment = [
      { name = "ASPNETCORE_URLS",       value = "http://+:8080" },
      { name = "MongoDB__DatabaseName", value = "os" },
      { name = "RabbitMQ__Port",        value = "5672" },
      { name = "Jwt__Issuer",           value = "TechChallenger" },
      { name = "Jwt__Audience",         value = "TechChallenger" },
      { name = "SystemUserId",          value = var.system_user_id },
      { name = "Datadog__StatsdHost",   value = "localhost" },
      { name = "Datadog__StatsdPort",   value = "8125" },
    ]

    secrets = [
      { name = "MongoDB__ConnectionString", valueFrom = "${aws_secretsmanager_secret.os_service.arn}:MongoDB__ConnectionString::" },
      { name = "RabbitMQ__Host",            valueFrom = "${aws_secretsmanager_secret.os_service.arn}:RabbitMQ__Host::" },
      { name = "RabbitMQ__UserName",        valueFrom = "${aws_secretsmanager_secret.os_service.arn}:RabbitMQ__UserName::" },
      { name = "RabbitMQ__Password",        valueFrom = "${aws_secretsmanager_secret.os_service.arn}:RabbitMQ__Password::" },
      { name = "Jwt__Key",                  valueFrom = "${aws_secretsmanager_secret.os_service.arn}:Jwt__Key::" },
    ]

    logConfiguration = {
      logDriver = "awslogs"
      options = {
        "awslogs-group"         = aws_cloudwatch_log_group.os_service.name
        "awslogs-region"        = var.aws_region
        "awslogs-stream-prefix" = "ecs"
      }
    }
  }])

  tags = { Name = "os-service" }
}

resource "aws_ecs_service" "os_service" {
  name            = "os-service"
  cluster         = aws_ecs_cluster.main.id
  task_definition = aws_ecs_task_definition.os_service.arn
  desired_count   = 1
  launch_type     = "FARGATE"

  network_configuration {
    subnets          = aws_subnet.public[*].id
    security_groups  = [aws_security_group.ecs.id]
    assign_public_ip = true
  }

  load_balancer {
    target_group_arn = aws_lb_target_group.os_service.arn
    container_name   = "os-service"
    container_port   = 8080
  }

  depends_on = [aws_lb_listener.os_service]

  lifecycle {
    # O pipeline de deploy atualiza a task definition — Terraform não reverte.
    ignore_changes = [task_definition]
  }
}
