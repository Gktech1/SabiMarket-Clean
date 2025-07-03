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

public interface ISubscriptionPlanService
{
    Task<BaseResponse<string>> CreateSubscriptionPlan(CreateSubscriptionPlanDto dto);
    Task<BaseResponse<PaginatorDto<IEnumerable<SubscriptionPlan>>>> GetAllSubscriptionPlans(PaginationFilter filter);
    Task<BaseResponse<SubscriptionPlan>> GetSubscriptionPlanById(string Id);
    Task<BaseResponse<string>> UpdateSubscriptionPlan(UpdateSubscriptionPlanDto dto);
}
