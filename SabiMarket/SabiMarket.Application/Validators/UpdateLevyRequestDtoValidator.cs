using FluentValidation;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Domain.Enum;

public class UpdateLevyRequestDtoValidator : AbstractValidator<UpdateLevyRequestDto>
{
    public UpdateLevyRequestDtoValidator()
    {
        RuleFor(x => x.MarketId)
            .NotEmpty().WithMessage("Market ID is required")
            .MaximumLength(50).WithMessage("Market ID cannot exceed 50 characters");

        RuleFor(x => x.TraderId)
            .NotEmpty().WithMessage("Trader ID is required")
            .MaximumLength(50).WithMessage("Trader ID cannot exceed 50 characters");

        RuleFor(x => x.GoodBoyId)
            .NotEmpty().WithMessage("Good Boy ID is required")
            .MaximumLength(50).WithMessage("Good Boy ID cannot exceed 50 characters");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0")
            .LessThan(1000000).WithMessage("Amount cannot exceed 1,000,000");

        RuleFor(x => x.Period)
            .IsInEnum().WithMessage("Invalid payment period")
            .When(x => x.Period.HasValue);

        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("Invalid payment method")
            .When(x => x.PaymentMethod.HasValue);

        RuleFor(x => x.PaymentStatus)
            .IsInEnum().WithMessage("Invalid payment status")
            .When(x => x.PaymentStatus.HasValue);

        When(x => x.HasIncentive, () =>
        {
            RuleFor(x => x.IncentiveAmount)
                .NotNull().WithMessage("Incentive amount is required when incentive is enabled")
                .GreaterThan(0).WithMessage("Incentive amount must be greater than 0")
                .LessThan(x => x.Amount)
                .WithMessage("Incentive amount cannot be greater than the levy amount");
        });

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}