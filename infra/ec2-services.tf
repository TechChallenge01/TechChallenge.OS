# EC2 t3.micro rodando MongoDB + RabbitMQ via Docker
# Fica na VPC, acessível pelos ECS tasks via IP privado.

data "aws_ami" "amazon_linux_2023" {
  most_recent = true
  owners      = ["amazon"]

  filter {
    name   = "name"
    values = ["al2023-ami-*-x86_64"]
  }

  filter {
    name   = "virtualization-type"
    values = ["hvm"]
  }
}

# ─── Security Group ────────────────────────────────────────────────────────────

resource "aws_security_group" "services" {
  name        = "${var.project_name}-services-sg"
  description = "MongoDB e RabbitMQ — acesso restrito aos ECS tasks"
  vpc_id      = aws_vpc.main.id

  ingress {
    description     = "MongoDB dos ECS tasks"
    from_port       = 27017
    to_port         = 27017
    protocol        = "tcp"
    security_groups = [aws_security_group.ecs.id]
  }

  ingress {
    description     = "RabbitMQ AMQP dos ECS tasks"
    from_port       = 5672
    to_port         = 5672
    protocol        = "tcp"
    security_groups = [aws_security_group.ecs.id]
  }

  ingress {
    description = "RabbitMQ Management UI (acesso externo para debug)"
    from_port   = 15672
    to_port     = 15672
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = { Name = "${var.project_name}-services-sg" }
}

# ─── EC2 Instance ─────────────────────────────────────────────────────────────

resource "aws_instance" "services" {
  ami                         = data.aws_ami.amazon_linux_2023.id
  instance_type               = "t3.micro"
  subnet_id                   = aws_subnet.public[0].id
  vpc_security_group_ids      = [aws_security_group.services.id]
  user_data_replace_on_change = true

  # SENHAS: use apenas caracteres alfanuméricos + hífen/underline.
  # Caracteres especiais como $, !, & podem quebrar o shell no user_data.
  user_data = <<-EOF
    #!/bin/bash
    set -e
    dnf update -y
    dnf install -y docker
    systemctl start docker
    systemctl enable docker
    mkdir -p /data/mongodb /data/rabbitmq

    # MongoDB 7 — banco de dados de documento
    docker run -d \
      --name mongodb \
      --restart always \
      -p 27017:27017 \
      -v /data/mongodb:/data/db \
      -e MONGO_INITDB_ROOT_USERNAME=admin \
      -e MONGO_INITDB_ROOT_PASSWORD=${var.mongodb_root_password} \
      mongo:7.0

    # RabbitMQ 3.13 — mensageria da Saga
    docker run -d \
      --name rabbitmq \
      --restart always \
      -p 5672:5672 \
      -p 15672:15672 \
      -v /data/rabbitmq:/var/lib/rabbitmq \
      -e RABBITMQ_DEFAULT_USER=${var.rabbitmq_username} \
      -e RABBITMQ_DEFAULT_PASS=${var.rabbitmq_password} \
      rabbitmq:3.13-management
    EOF

  tags = { Name = "${var.project_name}-services" }
}
