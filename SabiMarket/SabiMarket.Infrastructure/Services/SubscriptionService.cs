using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.Interfaces;
using SabiMarket.Application.IRepositories;
using SabiMarket.Domain.Entities.WaiveMarketModule;
using SabiMarket.Domain.Exceptions;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Utilities;

namespace SabiMarket.Infrastructure.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IRepositoryManager _repositoryManager;
        private readonly ApplicationDbContext _applicationDbContext;
        public SubscriptionService(IHttpContextAccessor contextAccessor, IRepositoryManager repositoryManager, ApplicationDbContext applicationDbContext)
        {
            _contextAccessor = contextAccessor;
            _repositoryManager = repositoryManager;
            _applicationDbContext = applicationDbContext;
        }

        public async Task<BaseResponse<string>> CreateSubscription(CreateSubscriptionDto dto)
        {
            try
            {
                var loggedInUser = Helper.GetUserDetails(_contextAccessor);
                var checkActiveSubscription = await _applicationDbContext.Subscriptions.Where(x => x.SubscriberId == dto.SubscriberId).OrderByDescending(s => s.CreatedAt).FirstOrDefaultAsync();
                if (checkActiveSubscription is not null && checkActiveSubscription.SubscriptionEndDate > DateTime.UtcNow.AddHours(1))
                {
                    return ResponseFactory.Fail<string>(new ForbidException("User already an active subscription."), "User already an active subscription.");

                }
                if (checkActiveSubscription is not null && !checkActiveSubscription.IsActive && !checkActiveSubscription.IsAdminConfirmPayment)
                {
                    return ResponseFactory.Fail<string>(new ForbidException("User already an active subscription."), "User already an active subscription.");

                }
                var subscription = new Subscription
                {
                    Amount = dto.Amount,
                    ProofOfPayment = dto.ProofOfPayment ?? "",
                    SubscriberType = dto.SubscriberType,
                    SubscriberId = dto.SubscriberId ?? loggedInUser.Id, // FIXED: Use an existing user ID
                    IsSubscriberConfirmPayment = !string.IsNullOrEmpty(dto.ProofOfPayment) ? true : false,
                };

                _repositoryManager.SubscriptionRepository.AddSubscription(subscription);
                await _repositoryManager.SaveChangesAsync();

                return ResponseFactory.Success<string>("Success", "Subscription request sent succesfully.");

            }
            catch (Exception ex)
            {
                return ResponseFactory.Fail<string>($"Error: {ex.Message}");
            }
        }
        public async Task<BaseResponse<bool>> CheckActiveVendorSubscription(string userId)
        {
            var getUser = await _applicationDbContext.Vendors.Where(x => x.UserId == userId).OrderByDescending(s => s.CreatedAt).FirstOrDefaultAsync();
            var getSubscription = await _applicationDbContext.Subscriptions.Where(s => s.SubscriberId == userId).OrderByDescending(s => s.CreatedAt).FirstOrDefaultAsync();
            if (getSubscription == null)
            {
                return ResponseFactory.Fail<bool>(new NotFoundException("No subscription found for this Customer."), "No subscription found for this Customer.");
            }
            if (getUser == null)
            {
                return ResponseFactory.Fail<bool>(new NotFoundException("User is not a Customer."), "Customer not found.");
            }
            if (getSubscription.IsSubscriberConfirmPayment && !string.IsNullOrEmpty(getSubscription.ProofOfPayment))
            {
                return ResponseFactory.Success<bool>(true, "Your subscription is pending approval.");
            }
            if (!getUser.IsSubscriptionActive && getUser.SubscriptionEndDate < DateTime.UtcNow.AddHours(1))
            {
                return ResponseFactory.Fail<bool>("User does not have an active subscription");
            }
            return ResponseFactory.Success<bool>(true, "User has an active subscription.");

        }
        public async Task<BaseResponse<bool>> CheckActiveCustomerSubscription(string userId)
        {
            var getCustomer = await _applicationDbContext.Customers.Where(x => x.UserId == userId).OrderByDescending(s => s.CreatedAt).FirstOrDefaultAsync();
            var getSubscription = await _applicationDbContext.Subscriptions.Where(s => s.SubscriberId == userId).OrderByDescending(s => s.CreatedAt).FirstOrDefaultAsync();
            if (getSubscription == null)
            {
                return ResponseFactory.Fail<bool>(new NotFoundException("No subscription found for this Customer."), "No subscription found for this Customer.");
            }
            if (getCustomer == null)
            {
                return ResponseFactory.Fail<bool>(new NotFoundException("User is not a Customer."), "Customer not found.");
            }
            if (getSubscription.IsSubscriberConfirmPayment && !string.IsNullOrEmpty(getSubscription.ProofOfPayment))
            {
                return ResponseFactory.Success<bool>(true, "Your subscription is pending approval.");
            }
            if (!getCustomer.IsSubscriptionActive && getCustomer.SubscriptionEndDate < DateTime.UtcNow.AddHours(1))
            {
                return ResponseFactory.Fail<bool>("User does not have an active subscription");
            }
            return ResponseFactory.Success<bool>(true, "User has an active subscription.");
        }
        public async Task<BaseResponse<bool>> AdminConfirmSubscriptionPayment(string subscriptionId)
        {
            try
            {
                var loggedInUser = Helper.GetUserDetails(_contextAccessor);

                var getSubscriptions = await _applicationDbContext.Subscriptions.Where(x => x.Id == subscriptionId).Include(x => x.Subscriber).OrderByDescending(s => s.CreatedAt).FirstOrDefaultAsync();
                if (getSubscriptions == null)
                {
                    return ResponseFactory.Fail<bool>(new NotFoundException("Subscription not found."), "Subscription not found.");
                }
                if (!string.IsNullOrEmpty(getSubscriptions.SubscriptionActivatorId) && getSubscriptions.IsActive && getSubscriptions.IsActive)
                {
                    return ResponseFactory.Fail<bool>(new("Subscription has already been treated."), "Subscription has already been treated.");
                }
                getSubscriptions.IsAdminConfirmPayment = true;
                getSubscriptions.IsActive = true;
                getSubscriptions.SubscriptionStartDate = DateTime.UtcNow.AddHours(1);
                getSubscriptions.SubscriptionEndDate = DateTime.UtcNow.AddMonths(1);
                getSubscriptions.SubscriptionActivatorId = loggedInUser.Id;

                _applicationDbContext.SaveChanges();
                return ResponseFactory.Success<bool>(true, "Subscription confirmed successfully.");
            }
            catch (Exception ex)
            {
                return ResponseFactory.Fail<bool>($"Error: {ex.Message}");
            }
        }
        public async Task<BaseResponse<bool>> UserConfirmSubscriptionPayment(string subscriptionId)
        {
            var getSubscriptions = await _applicationDbContext.Subscriptions.Where(x => x.Id == subscriptionId).OrderByDescending(s => s.CreatedAt).FirstOrDefaultAsync();
            if (getSubscriptions == null)
            {
                return ResponseFactory.Fail<bool>(new NotFoundException("Subscription not found."), "Subscription not found.");
            }
            getSubscriptions.IsSubscriberConfirmPayment = true;
            _applicationDbContext.SaveChanges();
            return ResponseFactory.Success<bool>(true, "Subscription confirmed successfully.");
        }
        public async Task<BaseResponse<Subscription>> GetSubscriptionById(string subscriptionId)
        {
            var getSubscriptions = await _repositoryManager.SubscriptionRepository.GetSubscriptionById(subscriptionId, false);
            if (getSubscriptions == null)
            {
                return ResponseFactory.Fail<Subscription>(new NotFoundException("Subscription not found."), "Subscription not found.");
            }
            getSubscriptions.IsSubscriberConfirmPayment = true;
            _applicationDbContext.SaveChanges();
            return ResponseFactory.Success(getSubscriptions);
        }

        public async Task<BaseResponse<PaginatorDto<IEnumerable<Subscription>>>> GetAllSubscription(PaginationFilter filter)
        {
            var getSubscriptions = await _repositoryManager.SubscriptionRepository.GetPagedSubscription(filter);
            if (getSubscriptions == null)
            {
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<Subscription>>>(new NotFoundException("Subscription not found."), "Subscription not found.");
            }
            return ResponseFactory.Success(getSubscriptions);
        }
        public async Task<BaseResponse<PaginatorDto<IEnumerable<Subscription>>>> SearchSubscription(string searchString, PaginationFilter filter)
        {
            var getSubscriptions = await _repositoryManager.SubscriptionRepository.SearchSubscription(searchString, filter);
            if (getSubscriptions == null)
            {
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<Subscription>>>(new NotFoundException("Subscription not found."), "Subscription not found.");
            }
            return ResponseFactory.Success(getSubscriptions);
        }

        public async Task<BaseResponse<SubscriptionDashboadDetailsDto>> SubscriptionDashBoardDetails()
        {
            var getSubscriptions = await _repositoryManager.SubscriptionRepository.GetAllSubscriptionForExport(false);
            var totalSubscription = getSubscriptions.Count();
            var totalAmount = getSubscriptions.Select(x => x.Amount).Sum();
            var confirmSubscription = getSubscriptions.Where(x => x.IsAdminConfirmPayment).Count();
            var pendingSubscription = totalSubscription - confirmSubscription;
            var result = new SubscriptionDashboadDetailsDto
            {
                TotalAmount = totalAmount,
                TotalNumberOfSubscribers = totalSubscription,
                TotalNumberOfConfirmedSubscribers = confirmSubscription,
                TotalNumberOfPendingSubscribers = pendingSubscription,
            };
            return ResponseFactory.Success(result);
        }

    }
}
