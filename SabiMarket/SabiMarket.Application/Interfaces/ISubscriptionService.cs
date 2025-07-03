using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Domain.Entities.WaiveMarketModule;

namespace SabiMarket.Application.Interfaces;

public interface ISubscriptionService
{
    Task<BaseResponse<bool>> AdminConfirmSubscriptionPayment(string subscriptionId);
    Task<BaseResponse<bool>> CheckActiveCustomerSubscription(string userId);
    Task<BaseResponse<bool>> CheckActiveVendorSubscription(string userId);
    Task<BaseResponse<string>> CreateSubscription(CreateSubscriptionDto dto);
    Task<BaseResponse<PaginatorDto<IEnumerable<Subscription>>>> GetAllSubscription(PaginationFilter filter);
    Task<BaseResponse<Subscription>> GetSubscriptionById(string subscriptionId);
    Task<BaseResponse<PaginatorDto<IEnumerable<Subscription>>>> SearchSubscription(string searchString, PaginationFilter filter);
    Task<BaseResponse<SubscriptionDashboadDetailsDto>> SubscriptionDashBoardDetails();
    Task<BaseResponse<bool>> UserConfirmSubscriptionPayment(string subscriptionId);
}
