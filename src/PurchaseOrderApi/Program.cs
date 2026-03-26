using System.Reflection;
using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PurchaseOrderApi.Application.Services;
using PurchaseOrderApi.Application.Validators;
using PurchaseOrderApi.Domain.Interfaces;
using PurchaseOrderApi.Infrastructure.Data;
using PurchaseOrderApi.Infrastructure.Repositories;
using PurchaseOrderApi.Domain.Entities;
using PurchaseOrderApi.Domain.Enums;
using PurchaseOrderApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// --- Serviços ---

// Entity Framework Core com SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

// Injeção de dependência
builder.Services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
builder.Services.AddScoped<IUserService, UserService>();

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreatePurchaseOrderValidator>();

// Controllers com serialização JSON configurada
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Purchase Order API",
        Version = "v1",
        Description = "API REST para gerenciamento do processo de pedido de compras com fluxo de aprovação hierárquica. " +
                      "Desenvolvida como parte do desafio técnico IndustriALL."
    });

    // Inclui comentários XML na documentação do Swagger
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// --- Pipeline HTTP ---

// Middleware global de tratamento de exceções (primeiro na pipeline)
app.UseMiddleware<GlobalExceptionMiddleware>();

// Swagger disponível em todos os ambientes para facilitar testes
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Purchase Order API v1");
    options.RoutePrefix = string.Empty; // Swagger na raiz
});

app.UseHttpsRedirection();
app.MapControllers();

// Aplica migrations e seed de usuários automaticamente em desenvolvimento
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    // Seed: cria usuários padrão se a tabela estiver vazia
    if (!db.Users.Any())
    {
        db.Users.AddRange(
            new User("João Silva", UserProfile.Collaborator),
            new User("Maria Souza", UserProfile.Supplies),
            new User("Carlos Oliveira", UserProfile.Manager),
            new User("Ana Costa", UserProfile.Director)
        );
        db.SaveChanges();
    }
}

app.Run();

// Necessário para testes de integração
public partial class Program { }
