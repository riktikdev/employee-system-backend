using System;
using System.Linq;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Serilog;
using StackExchange.Redis;
using EmployeeApi.Data;
using EmployeeApi.Models;
using EmployeeApi.Repositories;
using EmployeeApi.Services;
using EmployeeApi.Utils;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

var logFile = Environment.GetEnvironmentVariable("SERILOG_LOG_FILE") ?? "logs/log-.txt";
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(logFile, rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
if (string.IsNullOrEmpty(connectionString))
{
    Log.Fatal("CONNECTION_STRING не задан в окружении");
    return;
}

var redisConnection = Environment.GetEnvironmentVariable("REDIS_CONNECTION");
if (string.IsNullOrEmpty(redisConnection))
{
    Log.Fatal("REDIS_CONNECTION не задан в окружении");
    return;
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString)
);

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    return ConnectionMultiplexer.Connect(redisConnection);
});

builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddSingleton<RedisCache>();
builder.Services.AddSingleton<SessionManager>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
app.UseSerilogRequestLogging();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");

app.UseRouting();

app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value;
    if (!path!.StartsWith("/auth", StringComparison.OrdinalIgnoreCase))
    {
        if (!context.Request.Headers.TryGetValue("X-Session-Token", out var token))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Session token is missing");
            return;
        }
        var sessionManager = context.RequestServices.GetRequiredService<SessionManager>();
        var session = await sessionManager.GetSessionAsync(token!);
        if (session == null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Invalid or expired session token");
            return;
        }
        context.Items["UserId"] = session.UserId;
        context.Items["UserRole"] = session.Role;
    }
    await next();
});

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    var anyAdmin = db.Users.Any(u => u.Role == "Administrator");
    
    if (!anyAdmin)
    {
        var adminEmployee = new Employee
        {
            FirstName   = "Admin",
            LastName    = "User",
            Position    = "Administrator",
            DateOfBirth = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Email       = "admin@example.com",
            Phone       = "0000000000"
        };
        db.Employees.Add(adminEmployee);
        db.SaveChanges();

        var adminUser = new User
        {
            Username     = "admin@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Role         = "Administrator",
            EmployeeId   = adminEmployee.Id
        };
        db.Users.Add(adminUser);
        db.SaveChanges();

        Log.Information("Создан admin: admin@example.com / admin123");
    }
}

app.Run();
