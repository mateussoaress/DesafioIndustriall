namespace PurchaseOrderApi.Application.DTOs.Responses;

/// <summary>
/// DTO padrão para respostas de erro da API.
/// Garante um formato consistente e seguro para mensagens de erro.
/// </summary>
public class ErrorResponse
{
    /// <summary>Código HTTP do erro.</summary>
    public int StatusCode { get; set; }

    /// <summary>Mensagem descritiva do erro.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Detalhes adicionais (erros de validação, por exemplo).</summary>
    public IEnumerable<string>? Details { get; set; }

    public ErrorResponse(int statusCode, string message, IEnumerable<string>? details = null)
    {
        StatusCode = statusCode;
        Message = message;
        Details = details;
    }
}
