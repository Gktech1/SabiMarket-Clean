using FluentValidation;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.Interfaces;

public class CreateMarketRequestValidator : AbstractValidator<CreateMarketRequestDto>
{
    public CreateMarketRequestValidator(ICaretakerRepository caretakerRepository)
    {
        RuleFor(x => x.MarketName)
            .NotEmpty()
            .WithMessage("Market name is required")
            .MaximumLength(100)
            .WithMessage("Market name cannot exceed 100 characters")
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Market name cannot be empty or whitespace");

        RuleFor(x => x.MarketType)
            .IsInEnum()
            .WithMessage("Please select a valid market type (Shop, Kiosk, or Open space)");

       /* RuleFor(x => x.CaretakerId)
            .NotEmpty()
            .WithMessage("Caretaker is required")
            .MustAsync(async (caretakerId, cancellation) =>
            {
                if (string.IsNullOrEmpty(caretakerId)) return false;
                return await caretakerRepository.ExistsAsync(caretakerId);
            })
            .WithMessage("Selected caretaker does not exist");*/
    }
}

public class UpdateMarketRequestValidator : AbstractValidator<UpdateMarketRequestDto>
{
    public UpdateMarketRequestValidator(ICaretakerRepository caretakerRepository)
    {
        RuleFor(x => x.MarketName)
            .MaximumLength(100)
            .WithMessage("Market name cannot exceed 100 characters")
            .Must(name => string.IsNullOrEmpty(name) || !string.IsNullOrWhiteSpace(name))
            .WithMessage("Market name cannot be whitespace");

        RuleFor(x => x.MarketType)
            .IsInEnum()
            .When(x => x.MarketType != default)
            .WithMessage("Please select a valid market type (Shop, Kiosk, or Open space)");

        RuleFor(x => x.CaretakerId)
            .MustAsync(async (caretakerId, cancellation) =>
            {
                if (string.IsNullOrEmpty(caretakerId)) return true;
                return await caretakerRepository.ExistsAsync(caretakerId);
            })
            .When(x => !string.IsNullOrEmpty(x.CaretakerId))
            .WithMessage("Selected caretaker does not exist");
    }
}