using System.Data;
using System.Security.Claims;
using iText.StyledXmlParser.Jsoup.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.Interfaces;
using SabiMarket.Application.IRepositories;
using SabiMarket.Domain.Entities.OrdersAndFeedback;
using SabiMarket.Domain.Entities.Supporting;
using SabiMarket.Domain.Entities.WaiveMarketModule;
using SabiMarket.Domain.Exceptions;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Utilities;
using Serilog;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace SabiMarket.Infrastructure.Services;

public class WaivedProductService : IWaivedProductService
{
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly IRepositoryManager _repositoryManager;
    private readonly IHttpContextAccessor _contextAccessor;
    public WaivedProductService(IRepositoryManager repositoryManager, IHttpContextAccessor contextAccessor, ApplicationDbContext applicationDbContext)
    {
        _repositoryManager = repositoryManager;
        _contextAccessor = contextAccessor;
        _applicationDbContext = applicationDbContext;
    }

    public async Task<BaseResponse<string>> CreateWaivedProduct(CreateWaivedProductDto dto)
    {
        try
        {
            var loggedInUser = Helper.GetUserDetails(_contextAccessor);

            var verifyVendor = ValiateVendor(loggedInUser.Id);
            if (!verifyVendor.Item1)
            {
                return ResponseFactory.Fail<string>(verifyVendor.Item2);

            }
            var validateCategory = await _applicationDbContext.ProductCategories.AnyAsync(x => x.Id.ToLower() == dto.CategoryId.ToLower());
            if (!validateCategory)
            {
                return ResponseFactory.Fail<string>("Product category is not valid.");

            }
            var waivedProd = new WaivedProduct
            {
                IsAvailbleForUrgentPurchase = dto.IsAvailbleForUrgentPurchase,
                ImageUrl = dto.ImageUrl,
                ProductName = dto.ProductName,
                Price = dto.Price,
                ProductCategoryId = dto.CategoryId,
                CurrencyType = dto.CurrencyType,
                VendorId = verifyVendor.Item2,
                IsActive = true,
            };
            _repositoryManager.WaivedProductRepository.AddWaivedProduct(waivedProd);
            await _repositoryManager.SaveChangesAsync();
            return ResponseFactory.Success("Success", "Waive Product Created Successfully.");
        }
        catch (Exception)
        {

            return ResponseFactory.Fail<string>("An Error Occurred. Try again later");

        }
    }
    private Tuple<bool, string> ValiateVendor(string userId)
    {
        var getVendorDetails = _applicationDbContext.Vendors.FirstOrDefault(x => x.UserId == userId);
        if (getVendorDetails is null || getVendorDetails.Id is null)
        {
            return Tuple.Create(false, "Vendor not found.");

        }
        if (!getVendorDetails.IsActive)
        {
            return Tuple.Create(false, "Vendor is disabled.");

        }
        if (!getVendorDetails.IsSubscriptionActive)
        {
            return Tuple.Create(false, "Vendor does not have an active subscription.");

        }
        if (!getVendorDetails.IsVerified)
        {
            return Tuple.Create(false, "Vendor is not verified.");

        }
        return Tuple.Create(true, getVendorDetails.Id);
    }

    /* public async Task<BaseResponse<PaginatorDto<IEnumerable<WaivedProduct>>>> GetAllWaivedProducts(
      string category,
      PaginationFilter paginationFilter)
     {
         var correlationId = Guid.NewGuid().ToString();
         try
         {
             var query = _applicationDbContext.WaivedProducts.AsQueryable();

             if (!string.IsNullOrEmpty(category))
             {
                 query = query.Where(p => p.ProductCategoryId == category);
             }

             // Use the Paginate extension method
             var paginatedResult = await query.Paginate(paginationFilter);
            *//* // Create paginated response directly from entity
             var paginatedResult = new PaginatorDto<IEnumerable<WaivedProduct>>
             {
                 PageItems = result.PageItems,
                 PageSize = result.PageSize,
                 CurrentPage = result.CurrentPage,
                 NumberOfPages = result.NumberOfPages
             };
 *//*

             return ResponseFactory.Success(paginatedResult, "Waived products retrieved successfully");
         }
         catch (Exception ex)
         {

             //_logger.LogError(ex, "Error retrieving waived products: {ErrorMessage}", ex.Message);

             return ResponseFactory.Fail<PaginatorDto<IEnumerable<WaivedProduct>>>(
                 ex, "An unexpected error occurred while retrieving waived products"
             );
         }
     }*/

    public async Task<BaseResponse<PaginatorDto<IEnumerable<WaivedProduct>>>> GetAllWaivedProducts(
    string category,
    PaginationFilter paginationFilter)
    {
        try
        {
            // Fetch the waived products using pagination
            var result = await GetWaivedProductsAsync(category, paginationFilter);

            // Map WaivedProduct to WaivedProductDto
            var waivedProductDtos = result.PageItems.Select(p => new WaivedProduct
            {
                Id = p.Id,
                ProductName = p.ProductName,
                ImageUrl = p.ImageUrl,
                Price = p.Price,
                IsAvailbleForUrgentPurchase = p.IsAvailbleForUrgentPurchase,
                CurrencyType = p.CurrencyType,
                VendorId = p.VendorId,
                ProductCategoryId = p.ProductCategoryId
            });

            var paginatedResult = new PaginatorDto<IEnumerable<WaivedProduct>>
            {
                PageItems = waivedProductDtos,
                PageSize = result.PageSize,
                CurrentPage = result.CurrentPage,
                NumberOfPages = result.NumberOfPages,
                TotalItems = result.TotalItems,
            };


            return ResponseFactory.Success(paginatedResult, "Waived products retrieved successfully");
        }
        catch (Exception ex)
        {
            return ResponseFactory.Fail<PaginatorDto<IEnumerable<WaivedProduct>>>(ex,
                "An unexpected error occurred while retrieving waived products");
        }
    }

    private async Task<PaginatorDto<IEnumerable<WaivedProduct>>> GetWaivedProductsAsync(
   string category,
   PaginationFilter paginationFilter)
    {
        // Base query
        var query = _applicationDbContext.WaivedProducts.Include(c => c.ProductCategory).AsNoTracking();

        // Apply category filter if provided
        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(p => p.ProductCategory.Name.Contains(category));
        }

        // Order by product name
        query = query.OrderBy(p => p.ProductName);

        // Apply pagination
        return await query.Paginate(paginationFilter);
    }

    public async Task<BaseResponse<WaivedProduct>> GetWaivedProductById(string Id)
    {
        var waivedProduct = await _repositoryManager.WaivedProductRepository.GetWaivedProductById(Id, false);
        if (waivedProduct == null)
        {
            return ResponseFactory.Fail<WaivedProduct>(new NotFoundException("No Record Found."), "Record not found.");
        }

        return ResponseFactory.Success(waivedProduct);
    }
    public async Task<BaseResponse<List<ProductDetailsDto>>> GetUrgentPurchaseWaivedProduct(PaginationFilter filter)
    {
        var waivedProduct = await _applicationDbContext.WaivedProducts.Where(x => x.IsAvailbleForUrgentPurchase).Include(x => x.Vendor).Include(x => x.ProductCategory).OrderBy(x => x.ProductName).AsQueryable().Paginate(filter);
        if (waivedProduct == null)
        {
            return ResponseFactory.Fail<List<ProductDetailsDto>>(new NotFoundException("No Record Found."), "Record not found.");
        }
        var vendorDtos = waivedProduct.PageItems.Select(v => new ProductDetailsDto
        {
            ProductId = v.Id,
            IsAvailbleForUrgentPurchase = v.IsAvailbleForUrgentPurchase,
            Category = v.ProductCategory?.Name ?? "N/A", // Fallback if null
            CurrencyType = v.CurrencyType,
            Price = v.Price,
            ProductName = v.ProductName,

        }).ToList();
        return ResponseFactory.Success(vendorDtos);
    }
    public async Task<BaseResponse<int>> GetUrgentPurchaseWaivedProductCount()
    {
        var urgentpurchaseCount = await _applicationDbContext.WaivedProducts.Where(x => x.IsAvailbleForUrgentPurchase).CountAsync();
        if (urgentpurchaseCount == null)
        {
            return ResponseFactory.Fail<int>(new NotFoundException("No Record Found."), "Record not found.");
        }
        return ResponseFactory.Success(urgentpurchaseCount);
    }

    public async Task<BaseResponse<string>> UpdateProduct(UpdateWaivedProductDto dto)
    {
        var loggedInUser = Helper.GetUserDetails(_contextAccessor);

        var verifyVendor = ValiateVendor(loggedInUser.Id);
        if (!verifyVendor.Item1)
        {
            return ResponseFactory.Fail<string>(verifyVendor.Item2);

        }
        var product = await _repositoryManager.WaivedProductRepository.GetWaivedProductById(dto.ProductId, true);
        if (product == null)
        {
            return ResponseFactory.Fail<string>(new NotFoundException("No Record Found."), "Record not found.");
        }
        product.IsAvailbleForUrgentPurchase = dto.IsAvailbleForUrgentPurchase;
        product.ProductName = dto.ProductName;
        product.ImageUrl = dto.ImageUrl;
        product.Price = dto.Price;
        product.CurrencyType = dto.CurrencyType;
        product.UpdatedAt = DateTime.UtcNow.AddHours(1);
        product.ProductCategoryId = dto.CategoryId;

        //product.StockQuantity = dto.StockQuantity;
        //product.OriginalPrice = dto.OriginalPrice;
        //product.WaivedPrice = dto.WaivedPrice;
        //product.Description = dto.Description;
        await _repositoryManager.SaveChangesAsync();
        return ResponseFactory.Success("Success");

    }
    public async Task<BaseResponse<string>> DeleteProduct(string waiveProductId)
    {
        var loggedInUser = Helper.GetUserDetails(_contextAccessor);

        var verifyVendor = ValiateVendor(loggedInUser.Id);
        if (!verifyVendor.Item1)
        {
            return ResponseFactory.Fail<string>(verifyVendor.Item2);

        }
        var product = await _repositoryManager.WaivedProductRepository.GetWaivedProductById(waiveProductId, false);
        if (product == null)
        {
            return ResponseFactory.Fail<string>(new NotFoundException("No Record Found."), "Record not found.");
        }
        _applicationDbContext.WaivedProducts.Remove(product);
        await _repositoryManager.SaveChangesAsync();
        return ResponseFactory.Success("Product deleted successfully.");

    }

    public async Task<BaseResponse<PaginatorDto<IEnumerable<VendorDto>>>> GetVendorAndProducts(PaginationFilter filter)
    {
        try
        {
            var vendors = await _repositoryManager.VendorRepository.GetVendorsWithPagination(filter, false);
            if (vendors == null || !vendors.PageItems.Any())
            {
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<VendorDto>>>(new NotFoundException("No Record Found."), "Record not found.");
            }

            // Map Vendor to VendorDto to prevent circular reference
            var vendorDtos = vendors.PageItems.Select(v =>
            {
                var user = v.User;
                return new VendorDto
                {
                    Id = v.Id,
                    BusinessName = v.BusinessName,
                    VendorName = user != null ? $"{user.FirstName} {user.LastName}" : null,
                    Email = user?.Email,
                    PhoneNumber = user?.PhoneNumber,
                    LGA = v.LocalGovernment.Name,
                    UserAddress = user?.Address,
                    BusinessAddress = v.BusinessAddress,
                    VendorCurrencyType = v.VendorCurrencyType,
                    IsActive = v.IsActive,
                    ProfileImageUrl = v.User?.ProfileImageUrl,
                    Products = v.Products?.Select(p => new ProductDto
                    {
                        Id = p.Id,
                        ProductName = p.ProductName,
                        Price = p.Price
                    }).ToList()
                };
            }).ToList();


            var response = new PaginatorDto<IEnumerable<VendorDto>>
            {
                PageItems = vendorDtos,
                NumberOfPages = vendors.NumberOfPages,
                PageSize = vendors.PageSize,
                CurrentPage = vendors.CurrentPage,
                TotalItems = vendors.TotalItems,
            };

            return ResponseFactory.Success(response);
        }
        catch (Exception ex)
        {
            Log.Error("Exception occurred while getting vendors: " + ex.Message);
            return ResponseFactory.Fail<PaginatorDto<IEnumerable<VendorDto>>>(new Exception("An error occurred."), "Try again later.");
        }
    }
    public async Task<BaseResponse<PaginatorDto<IEnumerable<GetCustomerDetailsDto>>>> GetCustomers(PaginationFilter filter, string? searchString)
    {
        try
        {
            var customers = await _repositoryManager.CustomerRepository.GetCustomersWithPagination(filter, false, searchString);
            if (customers == null || !customers.PageItems.Any())
            {
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<GetCustomerDetailsDto>>>(new NotFoundException("No Record Found."), "Record not found.");
            }

            // Map Vendor to VendorDto to prevent circular reference
            var customersList = customers.PageItems.Select(v =>
            {
                var user = v.User;
                return new GetCustomerDetailsDto
                {
                    Id = v.Id,
                    FullName = user != null ? $"{user.FirstName} {user.LastName}" : null,
                    EmailAddress = user?.Email,
                    PhoneNumber = user?.PhoneNumber,
                    LGA = v?.LocalGovernment?.Name,
                    Address = user?.Address,
                    DateAdded = v.CreatedAt
                };
            }).ToList();


            var response = new PaginatorDto<IEnumerable<GetCustomerDetailsDto>>
            {
                PageItems = customersList,
                NumberOfPages = customers.NumberOfPages,
                PageSize = customers.PageSize,
                CurrentPage = customers.CurrentPage,
                TotalItems = customers.TotalItems,

            };

            return ResponseFactory.Success(response);
        }
        catch (Exception ex)
        {
            Log.Error("Exception occurred while getting vendors: " + ex.Message);
            return ResponseFactory.Fail<PaginatorDto<IEnumerable<GetCustomerDetailsDto>>>(new Exception("An error occurred."), "Try again later.");
        }
    }
    public async Task<BaseResponse<string>> BlockOrUnblockVendor(string vendorId)
    {
        try
        {
            var vendor = await _applicationDbContext.Vendors.FirstOrDefaultAsync(x => x.Id == vendorId);
            if (vendor == null)
                return ResponseFactory.Fail<string>(new Exception("An error occurred."), "Try again later.");

            vendor.IsActive = !vendor.IsActive;
            await _applicationDbContext.SaveChangesAsync();

            return ResponseFactory.Success("Success");
        }
        catch (Exception ex)
        {
            Log.Error($"Exception occurred while blocking or unblocking vendor with UserID ==> {vendorId}: " + ex.Message);
            return ResponseFactory.Fail<string>(new Exception("An error occurred."), "Try again later.");
        }
    }
    public async Task<BaseResponse<string>> RegisterCustomerPurchase(CustomerPurchaseDto dto)
    {
        try
        {
            var loggedInUser = Helper.GetUserDetails(_contextAccessor);

            var purchase = new CustomerPurchase
            {
                IsActive = true,
                ProofOfPayment = dto.ProofOfPayment,
                WaivedProductId = dto.WaivedProductId,
                DeliveryInfo = dto.DeliveryInfo,
            };
            _applicationDbContext.CustomerPurchases.Add(purchase);
            await _repositoryManager.SaveChangesAsync();
            return ResponseFactory.Success(purchase.Id, "Customer purchase created successfully.");

        }
        catch (Exception ex)
        {
            Log.Error("Exception occured while creating purchase record" + ex.Message);
            return ResponseFactory.Fail<string>("An Error Occurred. Try again later");


        }
    }
    public async Task<BaseResponse<string>> ConfirmCustomerPurchase(string id)
    {
        try
        {
            var loggedInUser = Helper.GetUserDetails(_contextAccessor);
            if (loggedInUser.Role.ToLower() != UserRoles.Admin.ToLower())
            {
                return ResponseFactory.Fail<string>(new UnauthorizedAccessException("You are not authorized for this action."), "Unautorised.");
            }
            var purchase = _applicationDbContext.CustomerPurchases.Find(id);
            if (purchase == null)
            {
                return ResponseFactory.Fail<string>($"Purchase with the Id: {id} could not be found");
            }
            purchase.IsPaymentConfirmed = true;
            await _repositoryManager.SaveChangesAsync();
            return ResponseFactory.Success("Success", "Customer purchase created successfully.");

        }
        catch (Exception ex)
        {
            Log.Error("Exception occured while confirming purchase record" + ex.Message);
            return ResponseFactory.Fail<string>("An Error Occurred. Try again later");
        }
    }

    public async Task<BaseResponse<string>> CreateProductCategory(string categoryName, string description)
    {
        try
        {
            var loggedInUser = Helper.GetUserDetails(_contextAccessor);
            if (_applicationDbContext.ProductCategories.Any(x => x.Name.ToLower() == categoryName.ToLower()))
            {
                return ResponseFactory.Fail<string>(new DuplicateNameException("Product already exist."), "Product already exist.");
            }
            var category = new ProductCategory
            {
                IsActive = true,
                Name = categoryName,
                Description = description
            };
            _applicationDbContext.ProductCategories.Add(category);
            await _repositoryManager.SaveChangesAsync();
            return ResponseFactory.Success("Success", "Category created successfully.");

        }
        catch (Exception ex)
        {
            Log.Error("Exception occured while creating product category" + ex.Message);
            return ResponseFactory.Fail<string>("An Error Occurred. Try again later");
        }
    }

    public async Task<BaseResponse<List<ProductCategoryDto>>> GetAllProductCategories()
    {

        var productCategory = await _applicationDbContext.ProductCategories.Where(x => x.IsActive).Select(x => new ProductCategoryDto
        {
            CategoryId = x.Id,
            CategoryName = x.Name,
        }).ToListAsync();

        if (productCategory == null)
        {
            return ResponseFactory.Fail<List<ProductCategoryDto>>(new NotFoundException("No Record Found."), "Record not found.");
        }

        return ResponseFactory.Success(productCategory);
    }
    public async Task<BaseResponse<string>> DeleteProductCategory(string id)
    {

        var productCategory = await _applicationDbContext.ProductCategories.FindAsync(id);

        if (productCategory == null)
        {
            return ResponseFactory.Fail<string>(new NotFoundException("No Record Found."), "Record not found.");
        }
        _applicationDbContext.Remove(productCategory);
        await _applicationDbContext.SaveChangesAsync();
        return ResponseFactory.Success("Record deleted successfully.");
    }

    //
    public async Task<BaseResponse<string>> CreateComplaint(string vendorId, string compalaint, string? imageUrl)
    {
        try
        {
            var loggedInUser = Helper.GetUserDetails(_contextAccessor);

            var vendor = _applicationDbContext.Vendors.FirstOrDefault(v => v.Id == vendorId);
            if (vendor == null)
            {
                return ResponseFactory.Fail<string>(new NotFoundException($"Vendor with Id: {vendorId} not Found."), "Vendor not found.");
            }
            var customerId = await _applicationDbContext.Customers.Where(x => x.UserId == loggedInUser.Id).FirstOrDefaultAsync();

            var complaint = new CustomerFeedback
            {
                Comment = compalaint,
                CustomerId = customerId.Id,
                Rating = 0,
                ImageUrl = imageUrl,
                VendorCode = vendor.VendorCode,
                VendorId = vendorId,
                IsActive = true
            };
            _applicationDbContext.CustomerFeedbacks.Add(complaint);
            await _repositoryManager.SaveChangesAsync();
            return ResponseFactory.Success("Success", "Feedback Created Successfully.");
        }
        catch (Exception)
        {

            return ResponseFactory.Fail<string>("An Error Occurred. Try again later");

        }
    }
    public async Task<BaseResponse<string>> CreateNextWaiveMarketDate(NextWaiveMarketDateDto nextWaiveMarketDate)
    {
        try
        {
            var lastDate = _applicationDbContext.WaiveMarketDates.OrderByDescending(x => x.CreatedAt).FirstOrDefault();
            if (lastDate != null)
            {
                lastDate.IsActive = false;
            }
            var nextDate = new WaiveMarketDates
            {
                NextWaiveMarketDate = nextWaiveMarketDate.MarketDate,
                WaiveMarketLocation = nextWaiveMarketDate.MarketVenue,
                IsActive = true
            };
            _applicationDbContext.WaiveMarketDates.Add(nextDate);
            await _repositoryManager.SaveChangesAsync();
            return ResponseFactory.Success("Success", "Next Waive Market Date Created Successfully.");
        }
        catch (Exception)
        {

            return ResponseFactory.Fail<string>("An Error Occurred. Try again later");

        }
    }
    public async Task<BaseResponse<NextWaiveMarketDateDto>> GetNextWaiveMarketDate()
    {
        try
        {
            var lastDate = await _applicationDbContext.WaiveMarketDates.Where(i => i.IsActive).OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync();
            if (lastDate == null)
            {
                return ResponseFactory.Fail<NextWaiveMarketDateDto>(new NotFoundException($"Next Waive Market not Found."), "Next Waive Market not Found.");
            };
            var response = new NextWaiveMarketDateDto
            {
                MarketDate = lastDate.NextWaiveMarketDate,
                MarketVenue = lastDate.WaiveMarketLocation
            };
            return ResponseFactory.Success(response, "Success");
        }
        catch (Exception)
        {

            return ResponseFactory.Fail<NextWaiveMarketDateDto>("An Error Occurred. Try again later");

        }
    }
    public async Task<BaseResponse<PaginatorDto<IEnumerable<WaiveMarketDates>>>> GetAllNextWaiveMarketDateRecords(PaginationFilter filter)
    {
        try
        {
            var records = await _applicationDbContext.WaiveMarketDates.Where(i => i.IsActive).OrderByDescending(x => x.CreatedAt).Paginate(filter);
            if (records == null || records.TotalItems < 1)
            {
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<WaiveMarketDates>>>(new NotFoundException($"Next Waive Market not Found."), "Next Waive Market not Found.");
            };

            return ResponseFactory.Success(records, "Success");
        }
        catch (Exception)
        {

            return ResponseFactory.Fail<PaginatorDto<IEnumerable<WaiveMarketDates>>>("An Error Occurred. Try again later");

        }
    }

    public async Task<BaseResponse<string>> UpdateComplaint(string complaintId, string vendorId, string complaintMsg, string? imageUrl)
    {
        var complaint = _applicationDbContext.CustomerFeedbacks.Find(complaintId);
        if (complaint == null)
        {
            return ResponseFactory.Fail<string>(new NotFoundException("No Record Found."), "Record not found.");
        }
        complaint.VendorId = vendorId;
        complaint.Comment = complaintMsg;
        complaint.ImageUrl = imageUrl;
        await _repositoryManager.SaveChangesAsync();
        return ResponseFactory.Success("Success");
    }
    public async Task<BaseResponse<string>> DeleteComplaint(string complaintId)
    {
        var complaint = _applicationDbContext.CustomerFeedbacks.Find(complaintId);
        if (complaint == null)
        {
            return ResponseFactory.Fail<string>(new NotFoundException("No Record Found."), "Record not found.");
        }
        _applicationDbContext.CustomerFeedbacks.Remove(complaint);
        await _repositoryManager.SaveChangesAsync();
        return ResponseFactory.Success("Success");
    }

    public async Task<BaseResponse<PaginatorDto<IEnumerable<CustomerFeedback>>>> GetAllComplaint(PaginationFilter filter)
    {
        var cusComplaint = await _applicationDbContext.CustomerFeedbacks.Paginate(filter);
        if (cusComplaint == null)
        {
            return ResponseFactory.Fail<PaginatorDto<IEnumerable<CustomerFeedback>>>(new NotFoundException("No Record Found."), "Record not found.");
        }

        return ResponseFactory.Success(cusComplaint);
    }
    public async Task<BaseResponse<CustomerFeedback>> GetCustomerFeedbackById(string Id)
    {
        var cusComplaint = _applicationDbContext.CustomerFeedbacks.Find(Id);
        if (cusComplaint == null)
        {
            return ResponseFactory.Fail<CustomerFeedback>(new NotFoundException("No Record Found."), "Record not found.");
        }

        return ResponseFactory.Success(cusComplaint);
    }

    public async Task<BaseResponse<string>> CustomerIndicateInterestForWaivedProduct(CustomerInterstForUrgentPurchase dto)
    {
        var response = new BaseResponse<string>();

        try
        {
            // 1. Get logged-in user
            var user = Helper.GetUserDetails(_contextAccessor);
            if (user.Id == null)
            {
                response.Status = false;
                response.Message = "Unauthorized access.";
                return response;
            }


            // Validate role
            if (user.Role?.ToLower() != "customer")
            {
                response.Status = false;
                response.Message = "Only customers can indicate interest.";
                return response;
            }
            //var waivedProduct = await _repositoryManager.WaivedProductRepository.GetWaivedProductById(Id, false);

            var product = await _applicationDbContext.WaivedProducts.FirstOrDefaultAsync(p => p.Id == dto.ProductId && p.VendorId == dto.VendorId);
            if (product == null)
            {
                response.Status = false;
                response.Message = "Product not found.";
                return response;
            }
            if (!product.IsAvailbleForUrgentPurchase)
            {
                response.Status = false;
                response.Message = "Product is not available for urgent purchase.";
                return response;
            }
            var getCustomer = _applicationDbContext.Customers.Where(x => x.UserId == user.Id).FirstOrDefault();
            if (getCustomer == null)
            {
                response.Status = false;
                response.Message = "Customer details not found.";
                return response;
            }
            //if (product.)
            //{
            //    response.Status = false;
            //    response.Message = $"Only {product.AvailableQuantity} item(s) available.";
            //    return response;
            //}

            // Notify vendor
            var notification = new WaiveMarketNotification
            {
                VendorId = dto.VendorId,
                CustomerId = getCustomer.Id,
                Message = $"A customer is interested in your {product.ProductName}.",
                IsActive = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            _applicationDbContext.WaiveMarketNotifications.Add(notification);
            await _applicationDbContext.SaveChangesAsync();

            response.Status = true;
            response.Message = "Interest recorded and vendor notified.";
            response.Data = "Success";
        }
        catch (Exception ex)
        {
            //_logger.LogError(ex, "Error indicating interest for waived product");
            response.Status = false;
            response.Message = "An error occurred while processing your request.";
        }

        return response;
    }

    public async Task<BaseResponse<List<NotificationDto>>> GetNotificationsAsync()
    {
        var response = new BaseResponse<List<NotificationDto>>();

        try
        {
            var user = Helper.GetUserDetails(_contextAccessor);
            if (user.Id == null)
            {
                response.Status = false;
                response.Message = "Unauthorized access.";
                return response;
            }

            List<WaiveMarketNotification> dbNotifications;

            if (user.Role?.ToLower() == "vendor")
            {
                var vendor = await _applicationDbContext.Vendors
                    .FirstOrDefaultAsync(x => x.UserId == user.Id);

                if (vendor == null)
                {
                    response.Status = false;
                    response.Message = "Vendor not found.";
                    return response;
                }

                dbNotifications = await _applicationDbContext.WaiveMarketNotifications
                    .Where(n => n.VendorId == vendor.Id)
                    .ToListAsync();
            }
            else if (user.Role.ToLower() == "customer")
            {
                var customer = await _applicationDbContext.Customers
                    .FirstOrDefaultAsync(x => x.UserId == user.Id);

                if (customer == null)
                {
                    response.Status = false;
                    response.Message = "Customer not found.";
                    return response;
                }

                dbNotifications = await _applicationDbContext.WaiveMarketNotifications
                    .Where(n => n.CustomerId == customer.Id)
                    .ToListAsync();
            }
            else
            {
                response.Status = false;
                response.Message = "Invalid user role.";
                return response;
            }

            var now = DateTime.UtcNow;

            var filteredNotifications = dbNotifications
                .Where(n =>
                    !n.IsActive || (
                        n.IsActive &&
                        !(
                            (now - n.CreatedAt).TotalHours > 24 ||
                            (n.UpdatedAt.HasValue && (now - n.UpdatedAt.Value).TotalHours > 6)
                        )
                    )
                )
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    VendorId = n.VendorId,
                    CustomerId = n.CustomerId,
                    Message = n.Message,
                    VendorResponse = n.VendorResponse,
                    IsActive = n.IsActive,
                    CreatedAt = n.CreatedAt,
                    UpdatedAt = n.UpdatedAt
                })
                .ToList();

            response.Status = true;
            response.Message = "Notifications retrieved successfully.";
            response.Data = filteredNotifications;
        }
        catch (Exception ex)
        {
            response.Status = false;
            response.Message = "Error retrieving notifications.";
        }

        return response;
    }



    public async Task<BaseResponse<bool>> CanProceedToPurchaseAsync(string notificationId, string vendorResponse)
    {
        var response = new BaseResponse<bool>();

        try
        {
            var user = Helper.GetUserDetails(_contextAccessor);
            if (user.Id == null || user.Role.ToLower() != "vendor")
            {
                response.Status = false;
                response.Message = "Only vendors can perform this action.";
                return response;
            }
            var getVendor = _applicationDbContext.Vendors.Where(x => x.UserId == user.Id).FirstOrDefault();
            if (getVendor == null || !getVendor.IsActive)
            {
                response.Status = false;
                response.Message = "Vendor not found or deactivated.";
                return response;
            }
            var notification = await _applicationDbContext.WaiveMarketNotifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.VendorId == getVendor.Id);

            if (notification == null)
            {
                response.Status = false;
                response.Message = "Notification not found.";
                return response;
            }

            // Update read status and time
            notification.IsActive = true;
            notification.VendorResponse = vendorResponse;
            notification.UpdatedAt = DateTime.UtcNow;

            _applicationDbContext.WaiveMarketNotifications.Update(notification);
            await _applicationDbContext.SaveChangesAsync();

            // Interpret response
            bool canProceed = vendorResponse.Trim().ToLower() == "yes";

            response.Status = true;
            response.Message = "Notification updated successfully.";
            response.Data = canProceed;
        }
        catch (Exception ex)
        {
            response.Status = false;
            response.Message = "Error updating notification.";
        }

        return response;
    }
    public async Task<BaseResponse<WaiveMarketNotification>> GetNotificationByIdAsync(string notificationId)
    {
        var response = new BaseResponse<WaiveMarketNotification>();

        try
        {
            var user = Helper.GetUserDetails(_contextAccessor);
            if (user.Id == null)
            {
                response.Status = false;
                response.Message = "Unauthorized access.";
                return response;
            }

            var getVendor = await _applicationDbContext.Vendors
                .FirstOrDefaultAsync(x => x.UserId == user.Id);

            var isVendor = user.Role?.ToLower() == "vendor" && getVendor != null;

            var notification = await _applicationDbContext.WaiveMarketNotifications
                .FirstOrDefaultAsync(n =>
                    n.Id == notificationId &&
                    (
                        (isVendor && n.VendorId == getVendor.Id) ||
                        (!isVendor && n.CustomerId == user.Id)
                    )
                );

            if (notification == null)
            {
                response.Status = false;
                response.Message = "Notification not found.";
                return response;
            }

            response.Status = true;
            response.Message = "Notification retrieved successfully.";
            response.Data = notification;
        }
        catch (Exception ex)
        {
            response.Status = false;
            response.Message = "An error occurred while retrieving the notification.";
        }

        return response;
    }

}
