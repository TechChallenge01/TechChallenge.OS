terraform {
  required_version = ">= 1.6"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }

  # Execute infra/bootstrap.sh uma vez antes de rodar terraform init
  # para criar o bucket S3 e a tabela DynamoDB de lock.
  backend "s3" {
    bucket         = "techchallenge-tfstate" # substitua por techchallenge-tfstate-<seu-account-id>
    key            = "techchallenge/terraform.tfstate"
    region         = "us-east-1"
    dynamodb_table = "techchallenge-tflock"
    encrypt        = true
  }
}

provider "aws" {
  region = var.aws_region

  default_tags {
    tags = {
      Project     = "TechChallenge"
      Environment = var.environment
      ManagedBy   = "Terraform"
    }
  }
}

data "aws_caller_identity" "current" {}

data "aws_availability_zones" "available" {
  state = "available"
}
