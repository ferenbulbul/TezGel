
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using StackExchange.Redis;
using TezGel.API.Middlewares;
using TezGel.Application.DTOs.Auth;
using TezGel.Application.Interfaces;
using TezGel.Application.Interfaces.Repositories;
using TezGel.Application.Interfaces.Services;
using TezGel.Application.Services;
using TezGel.Domain.Common;
using TezGel.Domain.Entities;
using TezGel.Infrastructure.HostedService;
using TezGel.Infrastructure.Messaging;
using TezGel.Infrastructure.Services;
using TezGel.Persistence.Context;
using TezGel.Persistence.Repositories;
using TokenOptions = TezGel.Domain.Common.TokenOptions;


var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
{
    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        serverOptions.ListenAnyIP(80);
    });
}


builder.Services.AddDbContext<TezGelDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddIdentity<AppUser, IdentityRole<Guid>>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
}).AddEntityFrameworkStores<TezGelDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "JwtBearer";
    options.DefaultChallengeScheme = "JwtBearer";
}).AddJwtBearer(options =>
{
    var tokenOptions = builder.Configuration.GetSection("TokenOptions").Get<TokenOptions>();
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = tokenOptions.Issuer,
        ValidAudience = tokenOptions.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenOptions.SecurityKey)),
        ClockSkew = TimeSpan.Zero
    };
});




builder.Services.Configure<TokenOptions>(builder.Configuration.GetSection("TokenOptions"));
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

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

builder.Services.AddHostedService<RabbitMqInitializerService>();

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")
        ?? builder.Configuration["Redis__Host"]
        ?? "localhost:6379"; // fallback

    Console.WriteLine($"[DEBUG] Redis bağlantı dizesi: {redisConnectionString}");

    var config = ConfigurationOptions.Parse(redisConnectionString);
    config.AbortOnConnectFail = false;
    config.ConnectRetry = 5;
    config.ConnectTimeout = 10000;

    return ConnectionMultiplexer.Connect(config);
});

// builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
// {
//     // Ortam değişkenini oku (Çalıştığını biliyoruz)
//     var redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");
//     Console.WriteLine($"--->>> Ortam Değişkeni Değeri: '{redisConnectionString}' <<<---");

//     if (string.IsNullOrEmpty(redisConnectionString))
//     {
//         // Bu artık olmamalı ama kontrol kalsın
//         throw new Exception("!!! Ortam değişkeni 'REDIS_CONNECTION_STRING' okunamadı veya boş.");
//     }

//     try
//     {
//         Console.WriteLine($"Redis bağlantı dizesi manuel ayrıştırılıyor: {redisConnectionString}");
//         var uri = new Uri(redisConnectionString);
//         var host = uri.Host; // gondola.proxy.rlwy.net
//         var port = uri.Port; // 29071 (Doğru port)
//         // Parolayı UserInfo'dan al ('default:parola' formatında gelir)
//         var password = string.IsNullOrEmpty(uri.UserInfo) ? null : uri.UserInfo.Split(new[] { ':' }, 2)[1];

//         Console.WriteLine($"Ayrıştırılan: Host={host}, Port={port}, Password={(string.IsNullOrEmpty(password) ? "Yok" : "Var")}");

//         var config = new ConfigurationOptions
//         {
//             // EndPoint'i manuel ve doğru port ile ekle
//             EndPoints = { { host, port } },
//             Password = password,
//             AbortOnConnectFail = false, // Bağlantı hatasında uygulamanın çökmesini engeller (başlangıçta)
//             ConnectTimeout = 10000, // Zaman aşımını biraz artırabilirsiniz (opsiyonel, milisaniye)
//             // Railway genellikle SSL kullanmaz iç ağda, ama gerekirse scheme'e göre ayarlayabilirsiniz:
//             // Ssl = uri.Scheme.Equals("rediss", StringComparison.OrdinalIgnoreCase),
//             Ssl = false // Şimdilik false olarak bırakalım
//         };

//         Console.WriteLine($"ConfigurationOptions: EndPoints={config.EndPoints.FirstOrDefault()}, Ssl={config.Ssl}, Password Set={!string.IsNullOrEmpty(config.Password)}");

//         // Manuel oluşturulan config ile bağlan
//         return ConnectionMultiplexer.Connect(config);
//     }
//     catch (UriFormatException ex)
//     {
//         Console.WriteLine($"Redis URL format hatası ({redisConnectionString}): {ex.Message}");
//         throw;
//     }
//     catch (Exception ex) // RedisConnectionException dahil diğer tüm hatalar
//     {
//         Console.WriteLine($"Redis konfigürasyon/bağlantı hatası ({redisConnectionString}): {ex.Message}");
//         // InnerException varsa onu da loglamak faydalı olabilir
//         if (ex.InnerException != null)
//         {
//             Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
//         }
//         throw;
//     }
// });

builder.Services.AddHostedService<ExpiredReservationBackgroundService>();
builder.Services.AddScoped<IMailService, MailService>();
builder.Services.AddScoped<IRedisService, RedisService>();

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ICustomerUserRepository, CustomerUserRepository>();
builder.Services.AddScoped<IBusinessUserRepository, BusinessUserRepository>();
builder.Services.AddScoped<IAuthService, AuthManager>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IRedisEmailVerificationService, RedisEmailVerificationService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductManager>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IActionRepository, ActionRepository>();
builder.Services.AddScoped<ILockService, RedisLockService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IBusinessUserRepository, BusinessUserRepository>();
builder.Services.AddScoped<ITimeZoneService, TimeZoneService>();
builder.Services.AddScoped<IMessagePublisher, RabbitMqPublisher>();




builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<BusinessRegisterRequest>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TezGel API", Version = "v1" });

    // JWT Authentication ayarları
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
    };

    c.AddSecurityDefinition("Bearer", securityScheme);

    var securityRequirement = new OpenApiSecurityRequirement
            {
                           {
                                new OpenApiSecurityScheme
                                {
                                    Reference = new OpenApiReference
                                    {
                                        Type = ReferenceType.SecurityScheme,
                                        Id = "Bearer"
                                    }
                                },
                                new string[] {}
                            }
            };

    c.AddSecurityRequirement(securityRequirement);
});


var app = builder.Build();



app.UseMiddleware<ErrorHandlingMiddleware>();
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TezGelDbContext>();

    var maxRetry = 10; // en fazla 10 kere dene
    var delay = TimeSpan.FromSeconds(3); // 3 saniye bekle her denemede

    for (var attempt = 0; attempt < maxRetry; attempt++)
    {
        try
        {
            dbContext.Database.Migrate();
            break; // Başarılı olursa loop'tan çık
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DB Retry {attempt + 1}] Database not ready yet: {ex.Message}");
            if (attempt == maxRetry - 1)
                throw; // 10 defa denedi, hala olmuyorsa fırlat
            Thread.Sleep(delay);
        }
    }
}
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();


