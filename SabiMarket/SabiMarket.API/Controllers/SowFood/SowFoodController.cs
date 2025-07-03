using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.Interfaces;

namespace SabiMarket.API.Controllers.SowFood
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SowFoodController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;
        private readonly ICloudinaryService _cloudinaryService;
        public SowFoodController(IServiceManager serviceManager, ICloudinaryService cloudinaryService)
        {
            _serviceManager = serviceManager;
            _cloudinaryService = cloudinaryService;
        }

        [HttpPost("CreateSowFoodProduct")]
        public async Task<IActionResult> CreateWaivedProducts(CreateWaivedProductDto dto)
        {
            var response = await _serviceManager.IWaivedProductService.CreateWaivedProduct(dto);
            if (!response.Status)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpGet("GetSowFoodProductById")]
        public async Task<IActionResult> GetWaivedProductById([FromQuery] string id)
        {
            var response = await _serviceManager.IWaivedProductService.GetWaivedProductById(id);
            if (!response.Status)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpGet("GetAllSowFoodProductsByCompany")]
        public async Task<IActionResult> GetWaivedProducts([FromQuery] string? category, [FromQuery] PaginationFilter filter)
        {
            var response = await _serviceManager.IWaivedProductService.GetAllWaivedProducts(category, filter);
            if (!response.Status)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpPut("UpdateSowFoodProduct")]
        public async Task<IActionResult> UpdateWaivedProducts(UpdateWaivedProductDto dto)
        {
            var response = await _serviceManager.IWaivedProductService.UpdateProduct(dto);
            if (!response.Status)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }
        [HttpDelete("RemoveSowFoodProduct")]
        public async Task<IActionResult> RemoveWaivedProducts(UpdateWaivedProductDto dto)
        {
            var response = await _serviceManager.IWaivedProductService.UpdateProduct(dto);
            if (!response.Status)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpPost("CreateSowFoodCompany")]
        public async Task<IActionResult> CreateSubscription(CreateSubscriptionDto dto)
        {
            var response = await _serviceManager.ISubscriptionService.CreateSubscription(dto);
            if (!response.Status)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpPost("UpdateSowFoodCompany")]
        public async Task<IActionResult> UpdateSowFoodCompany(CreateSubscriptionDto dto)
        {
            var response = await _serviceManager.ISubscriptionService.CreateSubscription(dto);
            if (!response.Status)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpGet("GetSowFoodCompanyById")]
        public async Task<IActionResult> CheckActiveVendorSubscription([FromQuery] string userId)
        {
            var response = await _serviceManager.ISubscriptionService.CheckActiveVendorSubscription(userId);
            if (!response.Status)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpGet("GetAllSowFoodCompanies")]
        public async Task<IActionResult> CheckActiveCustomerSubscription([FromQuery] string userId)
        {
            var response = await _serviceManager.ISubscriptionService.CheckActiveCustomerSubscription(userId);
            if (!response.Status)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpDelete("RemoveSowFoodCompany")]
        public async Task<IActionResult> RemoveSowFoodCompany([FromQuery] string userId)
        {
            var response = await _serviceManager.ISubscriptionService.CheckActiveCustomerSubscription(userId);
            if (!response.Status)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }


        [HttpPost("CreateSowFoodCompanyStaff")]
        public async Task<IActionResult> CreateSowFoodCompanyStaff(CreateSubscriptionDto dto)
        {
            var response = await _serviceManager.ISubscriptionService.CreateSubscription(dto);
            if (!response.Status)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpPost("UpdateSowFoodCompanyStaff")]
        public async Task<IActionResult> UpdateSowFoodCompanyStaff(CreateSubscriptionDto dto)
        {
            var response = await _serviceManager.ISubscriptionService.CreateSubscription(dto);
            if (!response.Status)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpGet("GetSowFoodCompanyStaffById")]
        public async Task<IActionResult> GetSowFoodCompanyStaffById([FromQuery] string userId)
        {
            var response = await _serviceManager.ISubscriptionService.CheckActiveVendorSubscription(userId);
            if (!response.Status)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpGet("GetAllSowFoodCompanyStaff")]
        public async Task<IActionResult> GetAllSowFoodCompanyStaff([FromQuery] string userId)
        {
            var response = await _serviceManager.ISubscriptionService.CheckActiveCustomerSubscription(userId);
            if (!response.Status)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpDelete("RemoveSowFoodCompanyStaff")]
        public async Task<IActionResult> RemoveSowFoodCompanyStaff([FromQuery] string userId)
        {
            var response = await _serviceManager.ISubscriptionService.CheckActiveCustomerSubscription(userId);
            if (!response.Status)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }


        [HttpPost("CreateSowFoodCompanySalesRecord")]
        public async Task<IActionResult> CreateSowFoodCompanySalesRecord(CreateSubscriptionDto dto)
        {
            var response = await _serviceManager.ISubscriptionService.CreateSubscription(dto);
            if (!response.Status)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpPost("UpdateSowFoodCompanySalesRecord")]
        public async Task<IActionResult> UpdateSowFoodCompanySalesRecord(CreateSubscriptionDto dto)
        {
            var response = await _serviceManager.ISubscriptionService.CreateSubscription(dto);
            if (!response.Status)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpGet("GetSowFoodCompanySalesRecordById")]
        public async Task<IActionResult> GetSowFoodCompanySalesRecordById([FromQuery] string userId)
        {
            var response = await _serviceManager.ISubscriptionService.CheckActiveVendorSubscription(userId);
            if (!response.Status)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpGet("GetAllSowFoodCompanySalesRecord")]
        public async Task<IActionResult> GetAllSowFoodCompanySalesRecord([FromQuery] string userId)
        {
            var response = await _serviceManager.ISubscriptionService.CheckActiveCustomerSubscription(userId);
            if (!response.Status)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpDelete("RemoveSowFoodCompanySalesRecord")]
        public async Task<IActionResult> RemoveSowFoodCompanySaleRecord([FromQuery] string userId)
        {
            var response = await _serviceManager.ISubscriptionService.CheckActiveCustomerSubscription(userId);
            if (!response.Status)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }


        [HttpGet("SearchCompanySalesRecord")]
        public async Task<IActionResult> SearchSubscription([FromQuery] string searchString, [FromQuery] PaginationFilter filter)
        {
            var response = await _serviceManager.ISubscriptionService.SearchSubscription(searchString, filter);
            if (!response.Status)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }
            return Ok(response);
        }

        /// <summary>
        /// /////////
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>

        [HttpPost("CreateSowFoodCompanyShelfItem")]
        public async Task<IActionResult> CreateSowFoodCompanyShelfItem(CreateSubscriptionDto dto)
        {
            var response = await _serviceManager.ISubscriptionService.CreateSubscription(dto);
            if (!response.Status)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpPost("UpdateSowFoodCompanyShelfItem")]
        public async Task<IActionResult> UpdateSowFoodCompanyShelfItem(CreateSubscriptionDto dto)
        {
            var response = await _serviceManager.ISubscriptionService.CreateSubscription(dto);
            if (!response.Status)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpGet("GetSowFoodCompanyShelfItemById")]
        public async Task<IActionResult> GetSowFoodCompanyShelfItemById([FromQuery] string userId)
        {
            var response = await _serviceManager.ISubscriptionService.CheckActiveVendorSubscription(userId);
            if (!response.Status)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpGet("GetAllSowFoodCompanyShelfItem")]
        public async Task<IActionResult> GetAllSowFoodCompanyShelfItem([FromQuery] string userId)
        {
            var response = await _serviceManager.ISubscriptionService.CheckActiveCustomerSubscription(userId);
            if (!response.Status)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpDelete("RemoveSowFoodCompanyShelfItem")]
        public async Task<IActionResult> RemoveSowFoodCompanyShelfItem([FromQuery] string userId)
        {
            var response = await _serviceManager.ISubscriptionService.CheckActiveCustomerSubscription(userId);
            if (!response.Status)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        [HttpGet("SearchCompanyShelfItem")]
        public async Task<IActionResult> SearchShelfItem([FromQuery] string searchString, [FromQuery] PaginationFilter filter)
        {
            var response = await _serviceManager.ISubscriptionService.SearchSubscription(searchString, filter);
            if (!response.Status)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }
            return Ok(response);
        }

        /// <summary>
        /// ////////////////////
        /// </summary>
        /// <param name="searchString"></param>
        /// <param name="filter"></param>
        /// <returns></returns>


        [HttpGet("SearchCompany")]
        public async Task<IActionResult> SearchCompany([FromQuery] string searchString, [FromQuery] PaginationFilter filter)
        {
            var response = await _serviceManager.ISubscriptionService.SearchSubscription(searchString, filter);
            if (!response.Status)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }
            return Ok(response);
        }



        [HttpGet("SearchProduct")]
        public async Task<IActionResult> SearchProduct([FromQuery] string searchString, [FromQuery] PaginationFilter filter)
        {
            var response = await _serviceManager.ISubscriptionService.SearchSubscription(searchString, filter);
            if (!response.Status)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }
            return Ok(response);
        }

        [HttpGet(" SearchStaffById")]
        public async Task<IActionResult> SearchStaff([FromQuery] string searchString, [FromQuery] PaginationFilter filter)
        {
            var response = await _serviceManager.ISubscriptionService.SearchSubscription(searchString, filter);
            if (!response.Status)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }
            return Ok(response);
        }

        [HttpGet(" SearchSalesRecord")]
        public async Task<IActionResult> SearchSales([FromQuery] string searchString, [FromQuery] PaginationFilter filter)
        {
            var response = await _serviceManager.ISubscriptionService.SearchSubscription(searchString, filter);
            if (!response.Status)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }
            return Ok(response);
        }

        [HttpGet(" SowFoodCompanyDashBoardDetails")]
        public async Task<IActionResult> SubscriptionDashBoardDetails()
        {
            var response = await _serviceManager.ISubscriptionService.SubscriptionDashBoardDetails();
            if (!response.Status)
            {
                // Handle different types of registration failures
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    StatusCodes.Status409Conflict => Conflict(response),
                    _ => BadRequest(response)
                };
            }
            return Ok(response);
        }

    }
}