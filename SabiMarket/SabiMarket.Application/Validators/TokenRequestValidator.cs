using FluentValidation;
using SabiMarket.Application.DTOs.Requests;

namespace SabiMarket.Application.Validators
{
    public class TokenRequestValidator : AbstractValidator<TokenRequestDto>
    {
        public TokenRequestValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh token is required")
                .MinimumLength(64).WithMessage("Invalid refresh token format")
                .MaximumLength(128).WithMessage("Invalid refresh token format");
        }
    }
}
