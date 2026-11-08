resource "aws_ecr_repository" "os_service" {
  name                 = "${var.project_name}-os-service"
  image_tag_mutability = "MUTABLE"

  image_scanning_configuration {
    scan_on_push = true
  }

  tags = { Name = "os-service" }
}

resource "aws_ecr_lifecycle_policy" "os_service" {
  repository = aws_ecr_repository.os_service.name

  policy = jsonencode({
    rules = [{
      rulePriority = 1
      description  = "Manter apenas as últimas 5 imagens"
      selection = {
        tagStatus   = "any"
        countType   = "imageCountMoreThan"
        countNumber = 5
      }
      action = { type = "expire" }
    }]
  })
}

