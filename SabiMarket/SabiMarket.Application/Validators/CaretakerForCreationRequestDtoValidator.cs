using FluentValidation;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Application.Validators
{
    public class CaretakerForCreationRequestDtoValidator : AbstractValidator<CaretakerForCreationRequestDto>
    {
        private readonly IMarketRepository _marketRepository;

        public CaretakerForCreationRequestDtoValidator(IMarketRepository marketRepository)
        {
            _marketRepository = marketRepository;

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required")
                .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters")
                .Matches(@"^[a-zA-Z\s]*$").WithMessage("Full name can only contain letters and spaces");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^\+?[0-9]{10,15}$").WithMessage("Please enter a valid phone number");

            RuleFor(x => x.EmailAddress)
                .NotEmpty().WithMessage("Email address is required")
                .EmailAddress().WithMessage("Please enter a valid email address");

            RuleFor(x => x.MarketId)
                .NotEmpty().WithMessage("Market selection is required")
                .MustAsync(async (marketId, cancellation) =>
                {
                    var marketExists = await _marketRepository.GetMarketByIdAsync(marketId, false);
                    return marketExists != null;
                }).WithMessage("Selected market does not exist");

            RuleFor(x => x.Gender)
                .NotEmpty().WithMessage("Gender selection is required")
                .Must(gender => new[] { "Male", "Female", "Other" }.Contains(gender))
                .WithMessage("Please select a valid gender");
        }
    }
}
