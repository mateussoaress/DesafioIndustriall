using FluentValidation;
using PurchaseOrderApi.Application.DTOs.Requests;

namespace PurchaseOrderApi.Application.Validators;

/// <summary>
/// Validador para ações de aprovação (aprovar, solicitar revisão, cancelar).
/// </summary>
public class ApprovalActionValidator : AbstractValidator<ApprovalActionRequest>
{
    public ApprovalActionValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("O ID do usuário é obrigatório.");

        RuleFor(x => x.ApproverLevel)
            .IsInEnum().WithMessage("O nível de aprovação informado é inválido.");
    }
}
