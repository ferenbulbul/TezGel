using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using TezGel.Application.Interfaces.Repositories;
using TezGel.Persistence.Context;
using TezGel.Persistence.Repositories;
using TezGel.ReservationConsumer;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<ReservationConsumerWorker>();
builder.Services.AddScoped<IActionRepository, ActionRepository>();

builder.Services.AddDbContext<TezGelDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
    
builder.Services.AddSingleton<IConnectionFactory>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new ConnectionFactory
    {
        HostName = config["RabbitMQ:HostName"],
        UserName = config["RabbitMQ:UserName"],
        Password = config["RabbitMQ:Password"]
    };
});

var host = builder.Build();
host.Run();
