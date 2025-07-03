using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SabiMarket.Domain.Enum;

namespace SabiMarket.Application.DTOs.Advertisement
{
    public class CreateAdvertisementRequestDto
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public string ImageUrl { get; set; }

        public string TargetUrl { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public string Language { get; set; }

        [Required]
        public string Location { get; set; }

        [Required]
        public string AdvertPlacement { get; set; }
    }

    public class UpdateAdvertisementRequestDto
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public string ImageUrl { get; set; }

        public string TargetUrl { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public string Language { get; set; }

        [Required]
        public string Location { get; set; }

        [Required]
        public string AdvertPlacement { get; set; }
    }

    public class AdvertisementResponseDto
    {
        public string Id { get; set; }
        public string VendorId { get; set; }
        public string VendorName { get; set; }
        public string VendorEmail { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string TargetUrl { get; set; }
        public string Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Price { get; set; }
        public string Language { get; set; }
        public string Location { get; set; }
        public string AdvertPlacement { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ViewCount { get; set; }
    }

    public class AdvertisementDetailResponseDto : AdvertisementResponseDto
    {
        public string AdminId { get; set; }
        public string AdminName { get; set; }
        public string PaymentProofUrl { get; set; }
        public string BankTransferReference { get; set; }
        public IEnumerable<AdvertisementLanguageDto> Translations { get; set; }
        public AdvertPaymentDto Payment { get; set; }
    }

    public class AdvertisementLanguageDto
    {
        public string Id { get; set; }
        public string AdvertisementId { get; set; }
        public string Language { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }

    public class AdvertPaymentDto
    {
        public string Id { get; set; }
        public string AdvertisementId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public string TransactionReference { get; set; }
        public DateTime PaymentDate { get; set; }
    }

    public class UploadPaymentProofRequestDto
    {
        [Required]
        public string AdvertisementId { get; set; }

        public string BankTransferReference { get; set; }
    }

    public class AdvertisementFilterRequestDto
    {
        public string SearchTerm { get; set; }
        public string Status { get; set; }
        public string Location { get; set; }
        public string Language { get; set; }
        public string AdvertPlacement { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public DateTime? EndDateFrom { get; set; }
        public DateTime? EndDateTo { get; set; }
    }
}