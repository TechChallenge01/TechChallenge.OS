output "os_service_url" {
  description = "URL pública do OS Service"
  value       = "http://${aws_lb.os_service.dns_name}"
}


output "ecr_os_repository_url" {
  description = "URL do repositório ECR do OS Service"
  value       = aws_ecr_repository.os_service.repository_url
}


output "ecs_cluster_name" {
  description = "Nome do cluster ECS"
  value       = aws_ecs_cluster.main.name
}

output "services_instance_public_ip" {
  description = "IP público do EC2 (MongoDB + RabbitMQ)"
  value       = aws_instance.services.public_ip
}

output "rabbitmq_management_url" {
  description = "RabbitMQ Management UI — http://<ip>:15672 (usuário e senha definidos nas variáveis)"
  value       = "http://${aws_instance.services.public_ip}:15672"
}

output "aws_account_id" {
  description = "ID da conta AWS"
  value       = data.aws_caller_identity.current.account_id
}
