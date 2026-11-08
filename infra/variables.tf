variable "aws_region" {
  description = "Região AWS"
  type        = string
  default     = "us-east-1"
}

variable "environment" {
  description = "Ambiente (prod, staging)"
  type        = string
  default     = "prod"
}

variable "project_name" {
  description = "Prefixo usado em todos os recursos"
  type        = string
  default     = "techchallenge"
}

variable "system_user_id" {
  description = "GUID usado como idUsuario nos use cases acionados por eventos"
  type        = string
  default     = "00000000-0000-0000-0000-000000000001"
}

# ─── MongoDB (EC2 interno) ────────────────────────────────────────────────────

variable "mongodb_root_password" {
  description = "Senha do usuário admin do MongoDB. Use apenas letras, números, hífen e underline."
  type        = string
  sensitive   = true
}

# ─── RabbitMQ (EC2 interno) ───────────────────────────────────────────────────

variable "rabbitmq_username" {
  description = "Usuário do RabbitMQ"
  type        = string
  sensitive   = true
}

variable "rabbitmq_password" {
  description = "Senha do RabbitMQ. Use apenas letras, números, hífen e underline."
  type        = string
  sensitive   = true
}

# ─── JWT ──────────────────────────────────────────────────────────────────────

variable "jwt_key" {
  description = "JWT signing key — mínimo 256 bits (openssl rand -base64 32)"
  type        = string
  sensitive   = true
}
