// using Microsoft.EntityFrameworkCore;
using product_service.Models;
using product_service.Services;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using MassTransit;
using Consumers;

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

