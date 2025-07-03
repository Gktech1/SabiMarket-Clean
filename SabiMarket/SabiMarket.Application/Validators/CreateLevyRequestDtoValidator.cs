using FluentValidation;
using SabiMarket.Application.DTOs.Requests;

public class CreateLevyRequestDtoValidator : AbstractValidator<CreateLevyRequestDto>
{
    public CreateLevyRequestDtoValidator()
    {
        RuleFor(x => x.MarketId)
            .NotEmpty().WithMessage("Market ID is required")
            .MaximumLength(50).WithMessage("Market ID cannot exceed 50 characters");

        RuleFor(x => x.TraderId)
            .NotEmpty().WithMessage("Trader ID is required")
            .MaximumLength(50).WithMessage("Trader ID cannot exceed 50 characters");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0")
            .LessThan(1000000).WithMessage("Amount cannot exceed 1,000,000");

        RuleFor(x => x.Period)
            .IsInEnum().WithMessage("Invalid payment period");

        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("Invalid payment method");

        RuleFor(x => x.IncentiveAmount)
            .GreaterThan(0).When(x => x.HasIncentive)
            .WithMessage("Incentive amount must be greater than 0 when incentive is enabled")
            .LessThan(x => x.Amount)
            .When(x => x.HasIncentive && x.IncentiveAmount.HasValue)
            .WithMessage("Incentive amount cannot be greater than the levy amount");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters");

        RuleFor(x => x.GoodBoyId)
            .NotEmpty().WithMessage("Good Boy ID is required")
            .MaximumLength(50).WithMessage("Good Boy ID cannot exceed 50 characters");

        RuleFor(x => x.CollectionDate)
            .NotEmpty().WithMessage("Collection date is required")
            .LessThanOrEqualTo(DateTime.Now).WithMessage("Collection date cannot be in the future");
    }
}