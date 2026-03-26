using FluentValidation;
using PurchaseOrderApi.Application.DTOs.Requests;

namespace PurchaseOrderApi.Application.Validators;

/// <summary>
/// Validador para envio do pedido para aprovação.
/// </summary>
public class SubmitRequestValidator : AbstractValidator<SubmitRequest>
{
    public SubmitRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("O ID do usuário é obrigatório.");
    }
}
