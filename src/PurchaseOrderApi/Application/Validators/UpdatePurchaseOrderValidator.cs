using FluentValidation;
using PurchaseOrderApi.Application.DTOs.Requests;

namespace PurchaseOrderApi.Application.Validators;

/// <summary>
/// Validador para atualização de pedido de compra.
/// </summary>
public class UpdatePurchaseOrderValidator : AbstractValidator<UpdatePurchaseOrderRequest>
{
    public UpdatePurchaseOrderValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("O pedido deve conter pelo menos 1 item.");

        RuleForEach(x => x.Items).SetValidator(new OrderItemRequestValidator());
    }
}
