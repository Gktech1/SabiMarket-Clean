using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.Interfaces;
using SabiMarket.Application.IRepositories;
using SabiMarket.Domain.Entities.WaiveMarketModule;
using SabiMarket.Domain.Exceptions;

namespace SabiMarket.Infrastructure.Services
{
    public class SubscriptionPlanService : ISubscriptionPlanService
    {
        private readonly IRepositoryManager _repositoryManager;
        public SubscriptionPlanService(IRepositoryManager repositoryManager)
        {
            _repositoryManager = repositoryManager;
        }

        public async Task<BaseResponse<string>> CreateSubscriptionPlan(CreateSubscriptionPlanDto dto)
        {
            var subscription = new SubscriptionPlan
            {
                Frequency = dto.Frequency,
                Amount = dto.Amount,
                IsActive = true
            };

            _repositoryManager.SubscriptionPlanRepository.AddSubscriptionPlan(subscription);
            await _repositoryManager.SaveChangesAsync();
            return ResponseFactory.Success("Success", "Subscription Created Successfully.");
        }

        public async Task<BaseResponse<PaginatorDto<IEnumerable<SubscriptionPlan>>>> GetAllSubscriptionPlans(PaginationFilter filter)
        {
            var subscriptionPlan = await _repositoryManager.SubscriptionPlanRepository.GetPagedSubscriptionPlan(filter);
            if (subscriptionPlan == null)
            {
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<SubscriptionPlan>>>(new NotFoundException("No Record Found."), "Record not found.");
            }

            return ResponseFactory.Success<PaginatorDto<IEnumerable<SubscriptionPlan>>>(subscriptionPlan);
        }
        public async Task<BaseResponse<SubscriptionPlan>> GetSubscriptionPlanById(string Id)
        {
            var subscriptionPlan = await _repositoryManager.SubscriptionPlanRepository.GetSubscriptionPlanById(Id, false);
            if (subscriptionPlan == null)
            {
                return ResponseFactory.Fail<SubscriptionPlan>(new NotFoundException("No Record Found."), "Record not found.");
            }

            return ResponseFactory.Success(subscriptionPlan);
        }

        public async Task<BaseResponse<string>> UpdateSubscriptionPlan(UpdateSubscriptionPlanDto dto)
        {
            var plan = await _repositoryManager.SubscriptionPlanRepository.GetSubscriptionPlanById(dto.Id, true);
            if (plan == null)
            {
                return ResponseFactory.Fail<string>(new NotFoundException("No Record Found."), "Record not found.");
            }

            plan.Frequency = dto.Frequency;
            plan.Amount = dto.Amount;

            await _repositoryManager.SaveChangesAsync();
            return ResponseFactory.Success("Success");

        }

    }
}
