# Os valores de host/connection-string são resolvidos em tempo de apply
# a partir do IP privado do EC2 criado em ec2-services.tf.

# ─── OS Service ───────────────────────────────────────────────────────────────

resource "aws_secretsmanager_secret" "os_service" {
  name                    = "${var.project_name}/os-service"
  recovery_window_in_days = 0

  tags = { Name = "os-service-secrets" }
}

resource "aws_secretsmanager_secret_version" "os_service" {
  secret_id = aws_secretsmanager_secret.os_service.id

  secret_string = jsonencode({
    "MongoDB__ConnectionString" = "mongodb://admin:${var.mongodb_root_password}@${aws_instance.services.private_ip}:27017/?authSource=admin"
    "RabbitMQ__Host"            = aws_instance.services.private_ip
    "RabbitMQ__UserName"        = var.rabbitmq_username
    "RabbitMQ__Password"        = var.rabbitmq_password
    "Jwt__Key"                  = var.jwt_key
  })
}
