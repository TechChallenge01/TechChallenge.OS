namespace Application.Interfaces;

public interface IMetricsService
{
    void Increment(string metricName, string[]? tags = null);
    void Gauge(string metricName, double value, string[]? tags = null);
    void Histogram(string metricName, double value, string[]? tags = null);
}
