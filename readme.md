# OpenTelemetry tests

### Getting things running

```
docker compose -f docker-compose.yml up
```

### Jaeger

OpenTelemetry provides a set of tools to capture the trace information and then make available to observability tools (like Zipkin and Jaeger). OpenTelemetry provides a set of [NuGet packages](https://github.com/open-telemetry/opentelemetry-dotnet) we can use to easily integration with .NET.

Jaeger UI: http://localhost:16686
