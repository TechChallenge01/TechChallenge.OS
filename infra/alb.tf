# ─── OS Service ───────────────────────────────────────────────────────────────

resource "aws_lb" "os_service" {
  name               = "${var.project_name}-os-alb"
  internal           = false
  load_balancer_type = "application"
  security_groups    = [aws_security_group.alb.id]
  subnets            = aws_subnet.public[*].id

  tags = { Name = "os-service-alb" }
}

resource "aws_lb_target_group" "os_service" {
  name        = "${var.project_name}-os-tg"
  port        = 8080
  protocol    = "HTTP"
  vpc_id      = aws_vpc.main.id
  target_type = "ip"

  health_check {
    enabled             = true
    healthy_threshold   = 2
    unhealthy_threshold = 3
    timeout             = 5
    interval            = 30
    path                = "/health"
    matcher             = "200"
  }

  tags = { Name = "os-service-tg" }
}

resource "aws_lb_listener" "os_service" {
  load_balancer_arn = aws_lb.os_service.arn
  port              = 80
  protocol          = "HTTP"

  default_action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.os_service.arn
  }
}
