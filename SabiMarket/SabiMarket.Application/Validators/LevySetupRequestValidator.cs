using FluentValidation;
using SabiMarket.Application.DTOs.Requests;

public class LevySetupRequestValidator : AbstractValidator<LevySetupRequestDto>
{
    public LevySetupRequestValidator()
    {
        RuleFor(x => x.MarketId)
            .NotEmpty().WithMessage("Market is required.");

        RuleFor(x => x.MarketType)
            .NotEmpty().WithMessage("Market Type is required.");

        RuleFor(x => x.TraderOccupancy)
            .NotEmpty().WithMessage("Trader Occupancy is required.");

       /* RuleFor(x => x.PaymentFrequencyDays)
            .GreaterThan(0).WithMessage("Payment Frequency must be at least 1 day.");*/

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0.");
    }
}
