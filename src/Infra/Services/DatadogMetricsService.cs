using Application.Interfaces;
using StatsdClient;

namespace Infra.Services;

public class DatadogMetricsService : IMetricsService
{
    private readonly IDogStatsd _statsd;

    public DatadogMetricsService(IDogStatsd statsd) => _statsd = statsd;

    public void Increment(string metric, string[]? tags = null)
        => _statsd.Increment(metric, tags: tags);

    public void Gauge(string metric, double value, string[]? tags = null)
        => _statsd.Gauge(metric, value, tags: tags);

    public void Histogram(string metric, double value, string[]? tags = null)
        => _statsd.Histogram(metric, value, tags: tags);
}
