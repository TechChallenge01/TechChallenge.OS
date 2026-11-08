resource "aws_cloudwatch_log_group" "os_service" {
  name              = "/ecs/os-service"
  retention_in_days = 7

  tags = { Name = "os-service-logs" }
}
