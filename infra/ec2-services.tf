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
  description = "MongoDB and RabbitMQ - access restricted to ECS tasks"
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
  iam_instance_profile        = "LabInstanceProfile"
  user_data_replace_on_change = true

  # SENHAS: use apenas caracteres alfanuméricos + hífen/underline.
  # Caracteres especiais como $, !, & podem quebrar o shell no user_data.
  user_data = <<-EOF
    #!/bin/bash
    exec > /var/log/user-data.log 2>&1
    set -ex

    sleep 15

    dnf update -y || true
    dnf install -y docker ec2-instance-connect
    systemctl enable docker
    systemctl start docker

    # Aguarda Docker daemon ficar pronto
    timeout 60 sh -c 'until docker info > /dev/null 2>&1; do sleep 2; done'

    mkdir -p /data/mongodb /data/rabbitmq

    docker rm -f mongodb rabbitmq 2>/dev/null || true

    # Pull com retry (3 tentativas)
    for i in 1 2 3; do docker pull mongo:7.0 && break || sleep 15; done
    for i in 1 2 3; do docker pull rabbitmq:3.13-management && break || sleep 15; done

    # MongoDB 7
    docker run -d \
      --name mongodb \
      --restart always \
      -p 27017:27017 \
      -v /data/mongodb:/data/db \
      -e MONGO_INITDB_ROOT_USERNAME=admin \
      -e MONGO_INITDB_ROOT_PASSWORD=${var.mongodb_root_password} \
      mongo:7.0

    # Aguarda MongoDB aceitar conexoes (inicializacao do usuario root leva ~30s)
    echo "Aguardando MongoDB inicializar..."
    timeout 180 sh -c 'until docker exec mongodb mongosh --username admin --password ${var.mongodb_root_password} --authenticationDatabase admin --eval "db.adminCommand(\"ping\")" > /dev/null 2>&1; do sleep 5; done'
    echo "MongoDB pronto"

    # Seed: cliente de teste (upsert - idempotente)
    docker exec mongodb mongosh \
      --username admin \
      --password ${var.mongodb_root_password} \
      --authenticationDatabase admin \
      --eval '
        db = db.getSiblingDB("cadastro");
        var r = db.clientes.updateOne(
          { "Cpf": "52998224725" },
          { $setOnInsert: {
            "_id": UUID("22222222-0000-0000-0000-000000000001"),
            "Nome": "Joao Pereira",
            "Cpf": "52998224725",
            "Email": "joao.pereira@email.com",
            "IdUsuarioCriacao": UUID("11111111-0000-0000-0000-000000000003"),
            "DataCriacao": new Date("2025-01-10T08:00:00Z"),
            "IdUsuarioAtualizacao": null,
            "DataAtualizacao": null,
            "Ativo": true
          }},
          { upsert: true }
        );
        print("Seed: upserted=" + r.upsertedCount + " matched=" + r.matchedCount);
      '

    # RabbitMQ 3.13
    docker run -d \
      --name rabbitmq \
      --restart always \
      -p 5672:5672 \
      -p 15672:15672 \
      -v /data/rabbitmq:/var/lib/rabbitmq \
      -e RABBITMQ_DEFAULT_USER=${var.rabbitmq_username} \
      -e RABBITMQ_DEFAULT_PASS=${var.rabbitmq_password} \
      rabbitmq:3.13-management

    docker ps
    echo "user-data concluido com sucesso"
    EOF

  tags = { Name = "${var.project_name}-services" }
}
