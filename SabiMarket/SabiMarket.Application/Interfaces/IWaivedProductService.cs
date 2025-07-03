using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Domain.Entities.OrdersAndFeedback;
using SabiMarket.Domain.Entities.WaiveMarketModule;

namespace SabiMarket.Application.Interfaces
{
    public interface IWaivedProductService
    {
        Task<BaseResponse<string>> BlockOrUnblockVendor(string vendorId);
        Task<BaseResponse<bool>> CanProceedToPurchaseAsync(string notificationId, string vendorResponse);
        Task<BaseResponse<string>> ConfirmCustomerPurchase(string id);
        Task<BaseResponse<string>> CreateComplaint(string vendorId, string compalaint, string? imageUrl);
        Task<BaseResponse<string>> CreateNextWaiveMarketDate(NextWaiveMarketDateDto nextWaiveMarketDate);
        Task<BaseResponse<string>> CreateProductCategory(string categoryName, string description);
        Task<BaseResponse<string>> CreateWaivedProduct(CreateWaivedProductDto dto);
        Task<BaseResponse<string>> CustomerIndicateInterestForWaivedProduct(CustomerInterstForUrgentPurchase dto);
        Task<BaseResponse<string>> DeleteComplaint(string complaintId);
        Task<BaseResponse<string>> DeleteProduct(string waiveProductId);
        Task<BaseResponse<string>> DeleteProductCategory(string id);
        Task<BaseResponse<PaginatorDto<IEnumerable<CustomerFeedback>>>> GetAllComplaint(PaginationFilter filter);
        Task<BaseResponse<List<ProductCategoryDto>>> GetAllProductCategories();
        Task<BaseResponse<PaginatorDto<IEnumerable<WaivedProduct>>>> GetAllWaivedProducts(string category, PaginationFilter paginationFilter);
        Task<BaseResponse<CustomerFeedback>> GetCustomerFeedbackById(string Id);
        Task<BaseResponse<NextWaiveMarketDateDto>> GetNextWaiveMarketDate();
        Task<BaseResponse<List<NotificationDto>>> GetNotificationsAsync();
        Task<BaseResponse<WaiveMarketNotification>> GetNotificationByIdAsync(string notificationId);
        Task<BaseResponse<List<ProductDetailsDto>>> GetUrgentPurchaseWaivedProduct(PaginationFilter filter);
        Task<BaseResponse<int>> GetUrgentPurchaseWaivedProductCount();
        Task<BaseResponse<PaginatorDto<IEnumerable<VendorDto>>>> GetVendorAndProducts(PaginationFilter filter);
        Task<BaseResponse<WaivedProduct>> GetWaivedProductById(string Id);
        Task<BaseResponse<string>> RegisterCustomerPurchase(CustomerPurchaseDto dto);
        Task<BaseResponse<string>> UpdateComplaint(string complaintId, string vendorId, string complaintMsg, string? imageUrl);
        Task<BaseResponse<string>> UpdateProduct(UpdateWaivedProductDto dto);
        Task<BaseResponse<PaginatorDto<IEnumerable<GetCustomerDetailsDto>>>> GetCustomers(PaginationFilter filter, string? searchString);
        Task<BaseResponse<PaginatorDto<IEnumerable<WaiveMarketDates>>>> GetAllNextWaiveMarketDateRecords(PaginationFilter filter);
    }
}
