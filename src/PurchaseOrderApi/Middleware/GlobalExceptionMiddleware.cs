using System.Text.Json;
using PurchaseOrderApi.Application.DTOs.Responses;

namespace PurchaseOrderApi.Middleware;

/// <summary>
/// Middleware global de tratamento de exceções.
/// Centraliza o tratamento de erros, garantindo respostas padronizadas e seguras.
/// Diferencia erros de validação, regras de negócio e erros internos.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            ArgumentException argEx => (StatusCodes.Status400BadRequest, argEx.Message),
            InvalidOperationException opEx => (StatusCodes.Status422UnprocessableEntity, opEx.Message),
            KeyNotFoundException notFoundEx => (StatusCodes.Status404NotFound, notFoundEx.Message),
            _ => (StatusCodes.Status500InternalServerError, "Ocorreu um erro interno. Tente novamente mais tarde.")
        };

        // Registra apenas erros internos com stack trace completo
        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Erro interno não tratado: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning("Erro de negócio/validação: {Message}", exception.Message);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new ErrorResponse(statusCode, message);
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
