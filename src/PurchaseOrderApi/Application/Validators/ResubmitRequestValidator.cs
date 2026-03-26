using FluentValidation;
using PurchaseOrderApi.Application.DTOs.Requests;

namespace PurchaseOrderApi.Application.Validators;

/// <summary>
/// Validador para reenvio do pedido após revisão.
/// </summary>
public class ResubmitRequestValidator : AbstractValidator<ResubmitRequest>
{
    public ResubmitRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("O ID do usuário é obrigatório.");

        When(x => x.Items != null && x.Items.Count > 0, () =>
        {
            RuleForEach(x => x.Items!).SetValidator(new OrderItemRequestValidator());
        });
    }
}
