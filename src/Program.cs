// using Microsoft.EntityFrameworkCore;
using product_service.Models;
using product_service.Services;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using MassTransit;
using Consumers;

using Prometheus;
using Sample.Web;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.Configure<ProductDatabaseSettings>(
    builder.Configuration.GetSection("ProductDatabase"));

builder.Services.AddSingleton<ProductsService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Masstransit Message Bus
string host = "localhost";
string username = "guest";
string password = "guest";

bool isRabbitMQAvailable = RabbitMQChecker.IsRabbitMQAvailable(host, username, password);

builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<GettingStartedConsumer>();

    if (isRabbitMQAvailable)
    {
        config.UsingRabbitMq((ctx, cfg) =>
        {
            cfg.Host(host, "/", h =>
            {
                h.Username(username);
                h.Password(password);
            });
            cfg.ConfigureEndpoints(ctx);
        });
    }
    else
    {
        config.UsingInMemory((ctx, cfg) =>
        {
            cfg.ConfigureEndpoints(ctx);
        });
    }
});

// monitoring
// Define an HTTP client that reports metrics about its usage, to be used by a sample background service.
builder.Services.AddHttpClient(SampleService.HttpClientName);

// Export metrics from all HTTP clients registered in services
builder.Services.UseHttpClientMetrics();

// A sample service that uses the above HTTP client.
builder.Services.AddHostedService<SampleService>();

builder.Services.AddHealthChecks()
    // Define a sample health check that always signals healthy state.
    .AddCheck<SampleHealthCheck>(nameof(SampleHealthCheck))
    // Report health check results in the metrics output.
    .ForwardToPrometheus();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapControllers();

app.UseRouting();

app.UseAuthorization();

// Capture metrics about all received HTTP requests.
app.UseHttpMetrics();

#pragma warning disable ASP0014 // Suggest using top level route registrations
app.UseEndpoints(endpoints =>
{
    // Enable the /metrics page to export Prometheus metrics.
    // Open http://localhost:5099/metrics to see the metrics.
    //
    // Metrics published in this sample:
    // * built-in process metrics giving basic information about the .NET runtime (enabled by default)
    // * metrics from .NET Event Counters (enabled by default, updated every 10 seconds)
    // * metrics from .NET Meters (enabled by default)
    // * metrics about requests made by registered HTTP clients used in SampleService (configured above)
    // * metrics about requests handled by the web app (configured above)
    // * ASP.NET health check statuses (configured above)
    // * custom business logic metrics published by the SampleService class
    endpoints.MapMetrics();
});
#pragma warning restore ASP0014 // Suggest using top level route registrations

app.Run();

