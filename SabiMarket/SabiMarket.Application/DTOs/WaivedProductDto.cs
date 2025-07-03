using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SabiMarket.Domain.Entities.WaiveMarketModule;
using SabiMarket.Domain.Enum;

namespace SabiMarket.Application.DTOs
{
    public class CreateWaivedProductDto
    {
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailbleForUrgentPurchase { get; set; }
        public string CategoryId { get; set; }
        public CurrencyTypeEnum CurrencyType { get; set; }

    }
    public class ProductDetailsDto
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public bool IsAvailbleForUrgentPurchase { get; set; }
        public string Category { get; set; }
        public string ImageUrl { get; set; }
        public CurrencyTypeEnum CurrencyType { get; set; }
        public decimal Price { get; set; }

        //public string Description { get; set; }
        //public decimal OriginalPrice { get; set; }
        //public decimal WaivedPrice { get; set; }
    }
    public class UpdateWaivedProductDto
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public bool IsAvailbleForUrgentPurchase { get; set; }
        public string CategoryId { get; set; }
        public string ImageUrl { get; set; }
        public CurrencyTypeEnum CurrencyType { get; set; }
        public decimal Price { get; set; }
        //public string Description { get; set; }
        //public decimal OriginalPrice { get; set; }
        //public decimal WaivedPrice { get; set; }
    }

    public class VendorDto
    {
        public string Id { get; set; }
        public string BusinessName { get; set; }
        public string VendorName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string LGA { get; set; }
        public string UserAddress { get; set; }
        public string BusinessAddress { get; set; }
        public string ProfileImageUrl { get; set; }
        public bool IsActive { get; set; }
        public CurrencyTypeEnum? VendorCurrencyType { get; set; }

        // Prevent circular references by not including the full User object
        public List<ProductDto> Products { get; set; } = new();
    }

    public class ProductDto
    {
        public string Id { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
    }

    public class CustomerInterstForUrgentPurchase
    {
        public string VendorId { get; set; }
        public string ProductId { get; set; }
    }
}
