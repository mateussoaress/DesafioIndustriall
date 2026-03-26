using FluentValidation;
using PurchaseOrderApi.Application.DTOs.Requests;

namespace PurchaseOrderApi.Application.Validators;

/// <summary>
/// Validador para criação de pedido de compra.
/// </summary>
public class CreatePurchaseOrderValidator : AbstractValidator<CreatePurchaseOrderRequest>
{
    public CreatePurchaseOrderValidator()
    {
        RuleFor(x => x.CreatorUserId)
            .NotEmpty().WithMessage("O ID do usuário criador é obrigatório.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("O pedido deve conter pelo menos 1 item.");

        RuleForEach(x => x.Items).SetValidator(new OrderItemRequestValidator());
    }
}

/// <summary>
/// Validador para cada item do pedido de compra.
/// </summary>
public class OrderItemRequestValidator : AbstractValidator<OrderItemRequest>
{
    public OrderItemRequestValidator()
    {
        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("O nome do produto é obrigatório.")
            .MaximumLength(300).WithMessage("O nome do produto deve ter no máximo 300 caracteres.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("A quantidade deve ser maior que zero.");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("O preço unitário deve ser maior que zero.");
    }
}
