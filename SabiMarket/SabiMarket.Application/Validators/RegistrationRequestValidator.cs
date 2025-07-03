using FluentValidation;
using Microsoft.AspNetCore.Identity;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Domain.Entities.UserManagement;

namespace SabiMarket.Application.Validators
{
    // Base Registration Validator
    public class RegistrationRequestValidator : AbstractValidator<RegistrationRequestDto>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public RegistrationRequestValidator(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;

            // Common rules for all registrations
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("A valid email address is required")
                .MustAsync(async (email, _) => !await EmailExists(email))
                    .WithMessage("Email is already registered");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
                .Matches("[0-9]").WithMessage("Password must contain at least one number")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^(\+234|234|0)[789][01]\d{8}$")
                .WithMessage("Please enter a valid Nigerian phone number");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address is required")
                .MaximumLength(200).WithMessage("Address cannot exceed 200 characters");

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Role is required")
                .Must(role => new[] { "VENDOR", "CUSTOMER", "ADVERTISER" }.Contains(role.ToUpper()))
                .WithMessage("Invalid role specified");

            // Role-specific validation
          /*  When(x => x.Role?.ToUpper() == "VENDOR", () =>
            {
                RuleFor(x => x.VendorDetails).SetValidator(new VendorDetailsValidator());
            });

            When(x => x.Role?.ToUpper() == "CUSTOMER", () =>
            {
                RuleFor(x => x.CustomerDetails).SetValidator(new CustomerDetailsValidator());
            });

            When(x => x.Role?.ToUpper() == "ADVERTISER", () =>
            {
                RuleFor(x => x.AdvertiserDetails).SetValidator(new AdvertiserDetailsValidator());
            });*/
        }

        private async Task<bool> EmailExists(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user != null;
        }
    }

    // Vendor Details Validator
  /*  public class VendorDetailsValidator : AbstractValidator<VendorDetailsDto>
    {
        public VendorDetailsValidator()
        {
            RuleFor(x => x.BusinessName)
                .NotEmpty().WithMessage("Business name is required")
                .MaximumLength(100).WithMessage("Business name cannot exceed 100 characters");

            RuleFor(x => x.BusinessType)
                .NotEmpty().WithMessage("Business type is required")
                .MaximumLength(50).WithMessage("Business type cannot exceed 50 characters");

            RuleFor(x => x.BusinessDescription)
                .MaximumLength(500).WithMessage("Business description cannot exceed 500 characters");

            RuleFor(x => x.LocalGovernmentId)
                .GreaterThan(0).WithMessage("Please select a valid local government");
        }
    }*/

    // Customer Details Validator
  /*  public class CustomerDetailsValidator : AbstractValidator<CustomerDetailsDto>
    {
        public CustomerDetailsValidator()
        {
            RuleFor(x => x.PreferredMarket)
                .MaximumLength(100).WithMessage("Preferred market cannot exceed 100 characters");

            RuleFor(x => x.LocalGovernmentId)
                .GreaterThan(0).WithMessage("Please select a valid local government");
        }
    }*/

    // Advertiser Details Validator
  /*  public class AdvertiserDetailsValidator : AbstractValidator<AdvertiserDetailsDto>
    {
        public AdvertiserDetailsValidator()
        {
            RuleFor(x => x.CompanyName)
                .NotEmpty().WithMessage("Company name is required")
                .MaximumLength(100).WithMessage("Company name cannot exceed 100 characters");

            RuleFor(x => x.BusinessType)
                .NotEmpty().WithMessage("Business type is required")
                .MaximumLength(50).WithMessage("Business type cannot exceed 50 characters");

            RuleFor(x => x.Website)
                .Must(BeAValidUrl).When(x => !string.IsNullOrEmpty(x.Website))
                .WithMessage("Please enter a valid website URL");
        }*/

      /*  private bool BeAValidUrl(string website)
        {
            return Uri.TryCreate(website, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }*/
   // }
}
