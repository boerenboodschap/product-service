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

builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<GettingStartedConsumer>(); // Add your consumer
    config.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        cfg.ConfigureEndpoints(ctx);
        // cfg.ReceiveEndpoint("/", ep =>
        // {
        //     ep.ConfigureConsumer<YourMessageConsumer>(ctx);
        // });
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.MapGet("/", () => "Hello World!");

// app.Run();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

