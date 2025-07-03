using FluentValidation;
using SabiMarket.Application.DTOs.Advertisement;
using System;

namespace SabiMarket.Application.Validators
{
    public class CreateAdvertisementValidator : AbstractValidator<CreateAdvertisementRequestDto>
    {
        public CreateAdvertisementValidator()
        {
            RuleFor(a => a.Title)
                .NotEmpty().WithMessage("Title is required")
                .MaximumLength(100).WithMessage("Title must not exceed 100 characters");

            RuleFor(a => a.Description)
                .NotEmpty().WithMessage("Description is required")
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

            RuleFor(a => a.StartDate)
                .NotEmpty().WithMessage("Start date is required")
                .GreaterThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("Start date must be in the future");

            RuleFor(a => a.EndDate)
                .NotEmpty().WithMessage("End date is required")
                .GreaterThan(a => a.StartDate).WithMessage("End date must be after start date");

            RuleFor(a => a.Price)
                .NotEmpty().WithMessage("Price is required")
                .GreaterThan(0).WithMessage("Price must be greater than zero");

            RuleFor(a => a.Language)
                .NotEmpty().WithMessage("Language is required");

            RuleFor(a => a.Location)
                .NotEmpty().WithMessage("Location is required");

            RuleFor(a => a.AdvertPlacement)
                .NotEmpty().WithMessage("Advertisement placement is required");
        }
    }

    public class UpdateAdvertisementValidator : AbstractValidator<UpdateAdvertisementRequestDto>
    {
        public UpdateAdvertisementValidator()
        {
            RuleFor(a => a.Id)
                .NotEmpty().WithMessage("Advertisement ID is required");

            RuleFor(a => a.Title)
                .NotEmpty().WithMessage("Title is required")
                .MaximumLength(100).WithMessage("Title must not exceed 100 characters");

            RuleFor(a => a.Description)
                .NotEmpty().WithMessage("Description is required")
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

            RuleFor(a => a.StartDate)
                .NotEmpty().WithMessage("Start date is required");

            RuleFor(a => a.EndDate)
                .NotEmpty().WithMessage("End date is required")
                .GreaterThan(a => a.StartDate).WithMessage("End date must be after start date");

            RuleFor(a => a.Price)
                .NotEmpty().WithMessage("Price is required")
                .GreaterThan(0).WithMessage("Price must be greater than zero");

            RuleFor(a => a.Language)
                .NotEmpty().WithMessage("Language is required");

            RuleFor(a => a.Location)
                .NotEmpty().WithMessage("Location is required");

            RuleFor(a => a.AdvertPlacement)
                .NotEmpty().WithMessage("Advertisement placement is required");
        }
    }

    public class UploadPaymentProofValidator : AbstractValidator<UploadPaymentProofRequestDto>
    {
        public UploadPaymentProofValidator()
        {
            RuleFor(p => p.AdvertisementId)
                .NotEmpty().WithMessage("Advertisement ID is required");
/*
            RuleFor(p => p.BankTransferReference)
                .NotEmpty().WithMessage("Bank transfer reference is required")
                .MaximumLength(50).WithMessage("Bank transfer reference must not exceed 50 characters");*/
        }
    }
}