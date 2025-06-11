using Microsoft.EntityFrameworkCore;
using TezGel.Application.Interfaces.Repositories;
using TezGel.Persistence.Context;
using TezGel.Persistence.Repositories;
using TezGel.ReservationConsumer;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<ReservationConsumerWorker>();
builder.Services.AddScoped<IActionRepository, ActionRepository>();

builder.Services.AddDbContext<TezGelDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
    
var host = builder.Build();
host.Run();
