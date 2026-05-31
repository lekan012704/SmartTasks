using Dapper;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SmartTask.Application.Command;
using SmartTask.Application.Command.Customer;
using SmartTask.Application.Command.Order;
using SmartTask.Application.Constants;
using SmartTask.Application.Dto;
using SmartTask.Application.Dto.Account;
using SmartTask.Application.Dto.Customer;
using SmartTask.Application.Dto.Logistics;
using SmartTask.Application.Dto.Order;
using SmartTask.Application.Dto.Paystack;
using SmartTask.Application.Dto.Role;
using SmartTask.Application.Enums;
using SmartTask.Application.Features.Orders.Commands;
using SmartTask.Application.Features.Orders.Queries;
using SmartTask.Application.Interfaces;
using SmartTask.Application.Query;
using SmartTask.Application.Query.Customer;
using SmartTask.Application.Query.Logistics;
using SmartTask.Application.Wrappers;
using SmartTask.Domain.Constants;
using SmartTask.Domain.Entities;
using SmartTask.Domain.Models;
using SmartTask.Persistence.Contexts;
using SmartTask.Shared.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static SmartTask.Domain.Constants.Permissions;

namespace SmartTask.Persistence.Repositories
{
    public class EntityMangerAsync : IEntityManagerAsync
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly ILogger<EntityMangerAsync> _logger;
        private readonly IDbConnection db;
        private readonly IAuditLogRepository _auditLogRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IShipBubbleService _shipBubbleService;
        private readonly IBackgroundJobClient _jobClient;
        private readonly IEmailService _mailService;
        private readonly IPaystackService _paystackService;
        private readonly AppSettings _appSettings;
        public EntityMangerAsync(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context, RoleManager<IdentityRole> roleManager, IAuthenticatedUserService authenticatedUserService, ILogger<EntityMangerAsync> logger, IDbConnection dbConnection, IAuditLogRepository auditLogRepo, IUnitOfWork unitOfWork, IHttpClientFactory httpClientFactory, IShipBubbleService shipBubbleService, IBackgroundJobClient jobClient, IEmailService mailService, IPaystackService paystackService, IOptions<AppSettings> appSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _roleManager = roleManager;
            _authenticatedUserService = authenticatedUserService;
            _logger = logger;
            db = dbConnection;
            _auditLogRepo = auditLogRepo;
            _unitOfWork = unitOfWork;
            _httpClientFactory = httpClientFactory;
            _shipBubbleService = shipBubbleService;
            _jobClient = jobClient;
            _mailService = mailService;
            _paystackService = paystackService;
            _appSettings = appSettings.Value;
        }


        public async Task<Response<CompanyResponse>> RegisterCompanyAsync(CompanyRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (await _unitOfWork.Companies.CompanyExistsAsync(request.CompanyName))
                {
                    _logger.LogWarning("Company '{CompanyName}' already exists", request.CompanyName);
                    return ApplicationConstants.FailureMessage<CompanyResponse>(
                        null,
                        $"Company '{request.CompanyName}' already exists."
                    );
                }

                var company = new Domain.Entities.Company
                {
                    Name = request.CompanyName,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    Address = request.Address,
                    Description = request.Description,
                    Type = (Application.Enums.CompanyType)request.CompanyType,
                    Country = request.Country
                };

                await _unitOfWork.Companies.AddAsync(company);

                var user = new ApplicationUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    EmailConfirmed = true,
                    CreatedBy = request.Email,
                    CompanyName = request.CompanyName,
                    CompanyId = company.Id,
                    IsActive = true,
                    DateCreated = DateTime.UtcNow,
                    Type = request.CompanyType,
                    PhoneNumber = request.PhoneNumber

                };
                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return ApplicationConstants.FailureMessage<CompanyResponse>(
                        null,
                        $"Failed: {string.Join(", ", result.Errors.Select(e => e.Description))}"
                    );
                }

                const string role = "CompanyAdmin";
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }

                await _userManager.AddToRoleAsync(user, role);

                // Commit changes using Unit of Work
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                var data = new CompanyResponse
                {
                    CompanyId = company.Id,
                    UserId = user.Id,
                    Email = request.Email
                };

                return ApplicationConstants.SuccessMessage(data, $"Company {request.CompanyName} registered successfully with user {request.Email}.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error registering company: {Message}", ex.Message);
                return ApplicationConstants.FailureMessage<CompanyResponse>(null, "An error occurred while registering the company.");
            }
        }



        public async Task<bool> UserExistsInCompanyAsync(string email)
        {
            var companyId = Guid.Parse(_authenticatedUserService.CompanyId!);
            return await _context.Users
                .AnyAsync(u =>
                    u.Email.ToLower() == email.ToLower()
                    && u.CompanyId == companyId
                );
        }

        public async Task<Response<UserResponseDto>> RegisterUserAsync(UserRequestDto request)
        {
            try
            {
                if (string.Equals(request.Role, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    return ApplicationConstants.FailureMessage<UserResponseDto>(null,
                        $"SuperAdmin role cannot be assigned to a '{request.Role}'.");
                }
                if (!Guid.TryParse(_authenticatedUserService.CompanyId, out var companyId))
                {
                    return ApplicationConstants.FailureMessage<UserResponseDto>(null,
                        "Unable to determine your company context.");
                }

                // ====================================================================
                // START FIX: Look up Company Name from CompanyId
                // ====================================================================

                // Assuming your DbContext (_context) has a DbSet for Companies (e.g., _context.Companies)
                // And your Company entity has a 'Name' property. Adjust if your entity is named differently.
                var company = await _unitOfWork.Companies.GetByIdAsync(companyId);
                if (company == null)
                {
                    _logger.LogError("RegisterUserAsync: Company not found for ID '{CompanyId}'", companyId);
                    return ApplicationConstants.FailureMessage<UserResponseDto>(null, "Unable to find your company details.");
                }
                var companyName = company.Name; // Get the company name as a string

                // ====================================================================
                // END FIX
                // ====================================================================

                if (await UserExistsInCompanyAsync(request.Email))
                {
                    _logger.LogWarning(
                        "RegisterUserAsync: User '{Email}' already exists in company '{CompanyId}'",
                        request.Email, companyId);

                    return ApplicationConstants.FailureMessage<UserResponseDto>(null,
                        $"A user with email '{request.Email}' already exists in your company.");
                }

                var newUser = new ApplicationUser
                {
                    UserName = request.UserName, // <-- Use request.UserName
                    Email = request.Email,
                    CompanyId = companyId,
                    CompanyName = companyName,        // <-- FIX 1: Set the CompanyName
                    FullName = request.FullName,      // <-- FIX 2: Set the FullName
                    PhoneNumber = request.PhoneNumber,  // <-- FIX 3: Set the PhoneNumber
                    CreatedBy = _authenticatedUserService.UserId,
                    IsActive = true,
                    DateCreated = request.DateCreated
                };

                _logger.LogInformation(
                    "RegisterUserAsync: Creating user '{Email}' under company '{CompanyId}'",
                    request.Email, companyId);

                var result = await _userManager.CreateAsync(newUser, request.Password);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    _logger.LogError(
                        "RegisterUserAsync: Failed to create user '{Email}': {Errors}",
                        request.Email, string.Join(", ", errors));

                    return ApplicationConstants.FailureMessage<UserResponseDto>(null,
                        string.Join(", ", errors));
                }

                if (!await _roleManager.RoleExistsAsync(request.Role))
                {
                    _logger.LogInformation("RegisterUserAsync: Creating role '{Role}'", request.Role);
                    await _roleManager.CreateAsync(new IdentityRole(request.Role));
                }

                await _userManager.AddToRoleAsync(newUser, request.Role);
                _logger.LogInformation(
                    "RegisterUserAsync: User '{Email}' assigned role '{Role}'",
                    request.Email, request.Role);

                var userResponse = new UserResponseDto
                {
                    Email = newUser.Email,
                    UserName = newUser.UserName,
                    FullName = newUser.FullName,
                    PhoneNumber = newUser.PhoneNumber,
                    Role = request.Role,
                    IsActive = request.IsActive,
                    DateCreated = request.DateCreated
                };

                return ApplicationConstants.SuccessMessage(userResponse,
                    "User registered successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user: {Message}", ex.Message);
                return ApplicationConstants.FailureMessage<UserResponseDto>(null,
                    "An error occurred while registering the user.");
            }
        }


        public async Task<Response<List<CompanyTypeDto>>> GetAllCompanyTypeAsync()
        {
            try
            {
                var companyTypes = Enum.GetValues(typeof(Application.Enums.CompanyType))
                    .Cast<Application.Enums.CompanyType>()
                    .Select(ct => new CompanyTypeDto
                    {
                        Id = (int)ct,
                        Name = ct.ToString()
                    })
                    .ToList();
                if (!companyTypes.Any())
                {
                    return ApplicationConstants.FailureMessage<List<CompanyTypeDto>>(null, "No company types found.");
                }
                return ApplicationConstants.SuccessMessage(companyTypes, "Company types retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving company types: {Message}", ex.Message);
                return ApplicationConstants.FailureMessage<List<CompanyTypeDto>>(null, "An error occurred while retrieving company types.");
            }

        }
        public async Task<Response<List<string>>> AddPermissionAsync(PermissionDto request)
        {
            try
            {
                // Validate role
                var role = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == request.RoleName);
                if (role == null)
                {
                    return ApplicationConstants.NotFoundMessage<List<string>>(null, $"Role '{request.RoleName}' not found.");
                }

                // Fetch requested permissions from IdentityContext
                var permissions = await _context.Permission
                    .Where(p => request.Permissions.Contains(p.Name))
                    .ToListAsync();

                var addedPermissions = new List<string>();

                foreach (var permission in permissions)
                {
                    var exists = await _context.RolePermission
                        .AnyAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id);

                    if (!exists)
                    {
                        _context.RolePermission.Add(new RolePermission
                        {
                            RoleId = role.Id,
                            PermissionId = permission.Id
                        });

                        addedPermissions.Add(permission.Name);
                    }
                }

                if (addedPermissions.Any())
                {
                    await _context.SaveChangesAsync();
                    return ApplicationConstants.SuccessMessage(addedPermissions, $"Added {addedPermissions.Count} permission(s) to role '{role.Name}'.");
                }

                return ApplicationConstants.FailureMessage(addedPermissions, $"No new permissions were added to role '{role.Name}' (all already assigned).");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding permissions to the role.");
                return ApplicationConstants.FailureMessage<List<string>>(null, $"An error occurred while adding permissions to role '{request.RoleName}'.");
            }
        }
        public async Task<Response<List<UserDto>>> GetUsersByCompanyAsync(GetUsersByCompany request)
        {
            var users = await _userManager.Users
                .Where(u => u.CompanyId == request.CompanyId)
                .ToListAsync();

            var result = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                result.Add(new UserDto
                {
                    Id = user.Id,
                    Email = user.Email ?? "No Email",
                    UserName = user.UserName ?? "No UserName",
                    Role = roles.FirstOrDefault() ?? "No Role",
                    IsActive = user.IsActive,
                    DateCreated = user.DateCreated,
                    PhoneNumber = user.PhoneNumber ?? "No Phone Number",
                    CreatedBy = _authenticatedUserService.UserName,
                    FullName = user.FullName ?? "No FullName"
                });
            }

            if (!result.Any())
            {
                return ApplicationConstants.FailureMessage<List<UserDto>>(null, "No users found for the specified company.");
            }

            return ApplicationConstants.SuccessMessage(result, "Users retrieved successfully.");
        }

        public async Task<Response<List<string>>> AddPermissionsToRoleAsync(PermissionDto request)
        {
            try
            {
                var role = await _roleManager.FindByNameAsync(request.RoleName);
                if (role == null)
                {
                    return ApplicationConstants.NotFoundMessage<List<string>>(null, $"Role '{request.RoleName}' not found.");
                }


                var permissionsInDb = await _context.Set<Permission>()
                    .Where(p => request.Permissions.Contains(p.Name))
                    .ToListAsync();

                if (!permissionsInDb.Any())
                {
                    return ApplicationConstants.FailureMessage<List<string>>(null, $"None of the specified permissions were found in the database.");
                }

                var permissionNamesInDb = permissionsInDb.Select(p => p.Name).ToList();
                var invalidPermissions = request.Permissions.Except(permissionNamesInDb).ToList();

                if (invalidPermissions.Any())
                {
                    _logger.LogWarning("Invalid permission names provided for role {RoleName}: {InvalidPermissions}", request.RoleName, string.Join(", ", invalidPermissions));

                }


                var addedPermissions = new List<string>();
                var existingPermissionIds = await _context.Set<RolePermission>()
                                            .Where(rp => rp.RoleId == role.Id)
                                            .Select(rp => rp.PermissionId)
                                            .ToListAsync();

                foreach (var permission in permissionsInDb)
                {
                    if (!existingPermissionIds.Contains(permission.Id))
                    {
                        _context.Set<RolePermission>().Add(new RolePermission
                        {
                            RoleId = role.Id,
                            PermissionId = permission.Id
                        });
                        addedPermissions.Add(permission.Name);
                    }
                }

                if (addedPermissions.Any())
                {
                    await _context.SaveChangesAsync();
                    string message = $"Added {addedPermissions.Count} permission(s) to role '{role.Name}'.";
                    if (invalidPermissions.Any())
                    {
                        message += $" Ignored invalid permissions: {string.Join(", ", invalidPermissions)}.";
                    }
                    return ApplicationConstants.SuccessMessage(addedPermissions, message);
                }

                string failureMsg = $"No new permissions were added to role '{role.Name}'.";
                if (invalidPermissions.Any())
                {
                    failureMsg += $" Invalid permissions specified: {string.Join(", ", invalidPermissions)}.";
                }
                else
                {
                    failureMsg += " All specified permissions were already assigned or invalid.";
                }
                return ApplicationConstants.SuccessMessage(new List<string>(), failureMsg);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding permissions to the role {RoleName}.", request.RoleName);
                return ApplicationConstants.FailureMessage<List<string>>(null, $"An internal error occurred while adding permissions to role '{request.RoleName}'.");
            }
        }

        public async Task<Response<string>> UpdateUserAsync(string Id, UpdateUserRequestDto requestDto)
        {
            try

            {
                var user = await _userManager.FindByIdAsync(Id);
                if (user == null)
                {
                    return new Response<string>($"User with ID {(Id)} not found.");
                }
                user.FullName = requestDto.FullName;
                user.PhoneNumber = requestDto.PhoneNumber;
                user.Email = requestDto.Email;
                user.UserName = requestDto.UserName;
                user.IsActive = true;
                user.DateCreated = requestDto.DateCreated;

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    return new Response<string>("Failed to update user");
                }
                if (!string.IsNullOrWhiteSpace(requestDto.Password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var passwordResult = await _userManager.ResetPasswordAsync(user, token, requestDto.Password);
                    if (!passwordResult.Succeeded)
                    {
                        return new Response<string>("Failed to update password.");
                    }
                }

                if (!string.IsNullOrWhiteSpace(requestDto.Role))
                {

                    if (!await _roleManager.RoleExistsAsync(requestDto.Role))
                    {
                        return new Response<string>($"Role '{requestDto.Role}' does not exist.");
                    }

                    var currentRoles = await _userManager.GetRolesAsync(user);
                    // Remove old roles
                    var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    if (!removeRolesResult.Succeeded)
                    {
                        return new Response<string>("Failed to remove existing roles.");
                    }

                    var addRoleResult = await _userManager.AddToRoleAsync(user, requestDto.Role);
                    if (!addRoleResult.Succeeded)
                    {
                        return new Response<string>("Failed to add new role.");
                    }
                }

                return new Response<string>(user.Id, "User updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while editing User to the  {UserName}.", requestDto.UserName);
                return new Response<string>($"An internal error occurred while adding permissions to role '{requestDto.UserName}'.");
            }
        }
        public async Task<Response<string>> DeleteUserAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new Response<string>($"User with ID {userId} not found.");
                }
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("SuperAdmin") || roles.Contains("CompanyAdmin"))
                {
                    return new Response<string>("A SuperAdmin or CompanyAdmin account cannot be deleted.");
                }
                var deleteResult = await _userManager.DeleteAsync(user);
                if (!deleteResult.Succeeded)
                {
                    return new Response<string>("Failed to delete user.");
                }
                return new Response<string>(userId, "User deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting user {UserId}.", userId);
                return new Response<string>($"An internal error occurred while deleting user '{userId}'.");
            }
        }
        public async Task<Response<string>> ActivateUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new Response<string>("User not found.");
            }

            if (user.IsActive == true)
            {
                return new Response<string>("User is already active.");
            }
            user.IsActive = true;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return new Response<string>("Failed to update user.");
            }

            return new Response<string>("User activated successfully.");
        }
        public async Task<Response<string>> DeactivateUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new Response<string>("User not found.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("SuperAdmin") || roles.Contains("CompanyAdmin"))
            {
                return new Response<string>("A SuperAdmin or CompanyAdmin account cannot be deactivated.");
            }


            if (user.IsActive == false)
            {
                return new Response<string>("User is already inactive.");
            }

            user.IsActive = false;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {

                return new Response<string>("Failed to update user.");
            }

            return new Response<string>("User deactivated successfully.");
        }
        public async Task<Guid> CreateOrderAsync(CreateOrderCommand request)
        {
            try
            {
                if (!Guid.TryParse(_authenticatedUserService.CompanyId, out var companyId))
                    throw new Exception("User is not authenticated.");

                // upsert customer by email within this company
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Email == request.CustomerEmail
                                           && c.CompanyId == companyId);

                if (customer == null)
                {
                    customer = new Customer
                    {
                        Id = Guid.NewGuid(),
                        Name = request.CustomerName,
                        Email = request.CustomerEmail,
                        PhoneNumber = request.CustomerPhone,
                        WhatsAppNumber = request.WhatsAppNumber,
                        CompanyId = companyId,
                        CreatedAt = DateTime.UtcNow,
                    };
                    _context.Customers.Add(customer);
                }
                else
                {
                    // update in case details changed
                    customer.Name = request.CustomerName;
                    customer.PhoneNumber = request.CustomerPhone;
                    customer.WhatsAppNumber = request.WhatsAppNumber;
                }

                decimal subtotal = request.OrderItems.Sum(item => item.Price * item.Quantity);
                var orderItemsJson = JsonConvert.SerializeObject(request.OrderItems);

                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    Status = OrderStatus.NewOrder,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CustomerName = request.CustomerName,
                    CustomerEmail = request.CustomerEmail,
                    CustomerPhone = request.CustomerPhone,   // new
                    WhatsAppNumber = request.WhatsAppNumber,
                    DeliveryAddress = request.DeliveryAddress,
                    OrderItemsJson = orderItemsJson,
                    Subtotal = subtotal,
                    DeliveryFee = request.DeliveryFee,
                    DriverName = request.DriverName,         // new
                    DriverPhone = request.DriverPhone,       // new
                    ApplicationUserId = _authenticatedUserService.UserId,
                    CustomerId = customer.Id,                // new
                };

                order.RecalculateTotal();
                _context.Order.Add(order);
                await _unitOfWork.SaveChangesAsync();
                return order.Id;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating order: {ex.Message}");
            }
        }

        public async Task<Unit> FulfillBatchManuallyAsync(FulfillBatchManuallyCommand request)
        {
            try
            {
                if (!Guid.TryParse(_authenticatedUserService.CompanyId, out var companyId))
                {
                    throw new Exception("User is not authenticated.");
                }
                var requestedIdCount = request.OrderIds.Distinct().Count();


                var ordersToUpdate = await _context.Order
                    .Where(o => o.ApplicationUserId == _authenticatedUserService.UserId && request.OrderIds.Contains(o.Id))
                    .ToListAsync();

                if (ordersToUpdate.Count != requestedIdCount)
                {
                    throw new Exception("One or more Order IDs were invalid or do not belong to you.");
                }
                var now = DateTime.UtcNow;
                foreach (var order in ordersToUpdate)
                {
                    order.Status = OrderStatus.InTransit;
                    order.ManualRiderName = request.ManualRiderName;
                    order.ManualTrackingInfo = request.ManualTrackingInfo;
                    order.UpdatedAt = now;
                }
                await _unitOfWork.SaveChangesAsync();

                return Unit.Value;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fulfilling batch: {ex.Message}");
            }
        }
        public async Task<List<OrderSummaryDto>> GetAllOrderAsync(GetAllOrdersQuery request)
        {

            if (!Guid.TryParse(_authenticatedUserService.CompanyId, out var companyId))
            {
                throw new Exception("User is not authenticated or CompanyId is missing.");
            }


            IQueryable<Order> query = _context.Order
                .AsNoTracking()
                .Where(p => p.ApplicationUserId == _authenticatedUserService.UserId);


            if (request.StatusIds != null && request.StatusIds.Any())
            {

                query = query.Where(o => request.StatusIds.Contains((int)o.Status));
            }


            if (!string.IsNullOrEmpty(request.Search))
            {
                string searchLower = request.Search.ToLower();
                query = query.Where(o =>
                    o.CustomerName!.ToLower().Contains(searchLower) ||
                    o.Id.ToString().Contains(searchLower));
            }


            if (request.MinAmount.HasValue)
            {
                query = query.Where(o => o.TotalDue >= request.MinAmount.Value);
            }
            if (request.MaxAmount.HasValue)
            {
                query = query.Where(o => o.TotalDue <= request.MaxAmount.Value);
            }

            if (request.StartDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt >= request.StartDate.Value);
            }
            if (request.EndDate.HasValue)
            {

                query = query.Where(o => o.CreatedAt <= request.EndDate.Value.AddDays(1));
            }


            var filteredOrders = await query
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new OrderSummaryDto
                {
                    Id = p.Id,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt,
                    CustomerName = p.CustomerName,
                    TotalDue = p.TotalDue
                }).ToListAsync();

            return filteredOrders;
        }
        public async Task<OrderDto> GetOrderByIdAsync(GetOrderByIdQuery request)
        {
            if (!Guid.TryParse(_authenticatedUserService.CompanyId, out var companyId))
            {
                throw new Exception("User is not authenticated.");
            }
            var order = await _context.Order.AsNoTracking().FirstOrDefaultAsync(p => p.Id == request.OrderId);
            if (order == null)
            {
                throw new Exception("Order not found.");
            }
            if (order.ApplicationUserId != _authenticatedUserService.UserId)
            {
                throw new Exception("You are not authorized to view this order.");
            }
            var response = new OrderDto
            {
                Id = order.Id,
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                CustomerName = order.CustomerName,
                WhatsAppNumber = order.WhatsAppNumber,
                DeliveryAddress = order.DeliveryAddress,
                Subtotal = order.Subtotal,
                TotalDue = order.TotalDue,
                TrackingNumber = order.TrackingNumber,
                LogisticsPartner = order.LogisticsPartner,
                ManualRiderName = order.ManualRiderName,
                ManualTrackingInfo = order.ManualTrackingInfo,
                OrderItems = JsonConvert.DeserializeObject<List<OrderItemDto>>(order.OrderItemsJson)
            };
            return response;
        }
        public async Task<CustomerDto> GetCustomerByIdAsync(GetCustomerByIdQuery request)
        {
            if (!Guid.TryParse(_authenticatedUserService.CompanyId, out var companyId))
                throw new Exception("User is not authenticated.");

            var customer = await _context.Customers
                .AsNoTracking()
                .Include(c => c.Orders.Where(o => !o.IsDeleted))
                .FirstOrDefaultAsync(c => c.Id == request.CustomerId && c.CompanyId == companyId);

            if (customer == null)
                throw new Exception("Customer not found.");

            return new CustomerDto
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber,
                WhatsAppNumber = customer.WhatsAppNumber,
                Address = customer.Address,
                CreatedAt = customer.CreatedAt,
                TotalOrders = customer.Orders.Count,
                TotalSpent = customer.Orders.Sum(o => o.TotalDue),
                LastOrderDate = customer.Orders.Any()
                                ? customer.Orders.Max(o => o.CreatedAt)
                                : null,
            };
        }

        public async Task<Unit> UpdateStatusAsync(UpdateOrderStatusCommand request)
        {
            try
            {
                if (!Guid.TryParse(_authenticatedUserService.CompanyId, out var companyId))
                {
                    throw new Exception("User is not authenticated.");
                }

                var order = await _context.Order
                    .FirstOrDefaultAsync(o => o.Id == request.OrderId);

                if (order == null)
                {
                    throw new Exception("Order not found.");
                }

                if (order.ApplicationUserId != _authenticatedUserService.UserId)
                {
                    throw new Exception("You are not authorized to modify this order.");
                }
                order.Status = request.NewStatus;
                order.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();
                return Unit.Value;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error Updating Status: {ex.Message}");
            }
        }
        public async Task<Unit> UpdateCustomerAsync(UpdateCustomerCommand request)
        {
            try
            {
                if (!Guid.TryParse(_authenticatedUserService.CompanyId, out var companyId))
                {
                    throw new Exception("User is not authenticated.");
                }

                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Id == request.Id && c.CompanyId == companyId);

                if (customer == null)
                {
                    throw new Exception("Customer not found.");
                }
                customer.Name = request.Update.Name;
                customer.Email = request.Update.Email;
                customer.PhoneNumber = request.Update.PhoneNumber;
                customer.WhatsAppNumber = request.Update.WhatsAppNumber;
                customer.Address = request.Update.Address;
                customer.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();
                return Unit.Value;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error Updating Customer: {ex.Message}");
            }
        }
        public async Task<List<CustomerDto>> GetAllCustomersAsync()
        {
            try
            {
                if (!Guid.TryParse(_authenticatedUserService.CompanyId, out var companyId))
                {
                    throw new Exception("User is not authenticated.");
                }
                var customers = await _context.Customers
                    .AsNoTracking()
                    .Where(c => c.CompanyId == companyId)
                    .Select(c => new CustomerDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Email = c.Email,
                        PhoneNumber = c.PhoneNumber,
                        WhatsAppNumber = c.WhatsAppNumber,
                        Address = c.Address,
                        TotalOrders = c.Orders.Count(o => !o.IsDeleted),
                        TotalSpent = c.Orders
        .Where(o => !o.IsDeleted)
        .Sum(o => (decimal?)o.TotalDue) ?? 0,
                        LastOrderDate = c.Orders
        .Where(o => !o.IsDeleted)
        .Max(o => (DateTime?)o.CreatedAt),
                        CreatedAt = c.CreatedAt,
                    })
                    .ToListAsync();
                return customers;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving customers: {ex.Message}");
            }
        }

        public async Task<DashboardStatsDto> GetDasboardAsync()
        {
            try
            {
                if (!Guid.TryParse(_authenticatedUserService.CompanyId, out var companyId))
                {
                    throw new Exception("User is not authenticated.");
                }

                var now = DateTime.UtcNow;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                var startOfLastMonth = startOfMonth.AddMonths(-1);
                var startOfYear = new DateTime(now.Year, 1, 1);

                var userOrders = _context.Order
                    .AsNoTracking()
                    .Where(o => o.ApplicationUserId == _authenticatedUserService.UserId);

                var totalSalesMonth = await userOrders
                    .Where(o => o.Status == OrderStatus.Delivered && o.CreatedAt >= startOfMonth)
                    .SumAsync(o => o.TotalDue);

                var lastMonthSales = await userOrders
                    .Where(o => o.Status == OrderStatus.Delivered
                             && o.CreatedAt >= startOfLastMonth
                             && o.CreatedAt < startOfMonth)
                    .SumAsync(o => o.TotalDue);

                decimal revenueGrowthPercentage = lastMonthSales == 0
                    ? 0
                    : ((totalSalesMonth - lastMonthSales) / lastMonthSales) * 100;

                var totalSalesYear = await userOrders
                    .Where(o => o.Status == OrderStatus.Delivered && o.CreatedAt >= startOfYear)
                    .SumAsync(o => o.TotalDue);

                var ordersToFulfill = await userOrders
                    .CountAsync(o => o.Status == OrderStatus.Paid || o.Status == OrderStatus.ReadyForDispatch);

                var pendingPayment = await userOrders
                    .CountAsync(o => o.Status == OrderStatus.PaymentPending);

                var totalOrdersMonth = await userOrders
                    .CountAsync(o => o.CreatedAt >= startOfMonth);

                var stats = new DashboardStatsDto
                {
                    TotalSalesMonth = totalSalesMonth,
                    TotalSalesYear = totalSalesYear,
                    OrdersToFulfill = ordersToFulfill,
                    PendingPayment = pendingPayment,
                    TotalOrdersMonth = totalOrdersMonth,
                    RevenueGrowthPercentage = Math.Round(revenueGrowthPercentage, 1)
                };

                return stats;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving dashboard stats: {ex.Message}");
            }
        }

        public async Task<Unit> DeleteOrderAsync(DeleteOrderCommand request)
        {
            try
            {
                if (!Guid.TryParse(_authenticatedUserService.CompanyId, out var companyId))
                {
                    throw new Exception("User is not authenticated.");
                }
                var order = await _context.Order
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(o => o.Id == request.OrderId);

                if (order == null || order.IsDeleted)
                {
                    throw new Exception("Order not found.");
                }

                if (order.ApplicationUserId != _authenticatedUserService.UserId)
                {
                    throw new Exception("You are not authorized to delete this order.");
                }

                order.IsDeleted = true;
                order.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();
                return Unit.Value;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error Updating Status: {ex.Message}");
            }
        }
        public async Task<Unit> DeleteCustomerAsync(DeleteCustomerCommand request)
        {
            try
            {
                if (!Guid.TryParse(_authenticatedUserService.CompanyId, out var companyId))
                {
                    throw new Exception("User is not authenticated.");
                }

                var customer = await _context.Customers
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(c => c.Id == request.CustomerId && c.CompanyId == companyId);

                if (customer == null || customer.IsDeleted)
                {
                    throw new Exception("Customer not found.");
                }

                customer.IsDeleted = true;
                customer.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                return Unit.Value;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting customer: {ex.Message}");
            }
        }
        //public async Task<BookDispatchResponseDto> DispatchOrderAsync(Guid orderId)
        //{
        //    try
        //    {
        //        if (!Guid.TryParse(_authenticatedUserService.CompanyId, out var companyId)) throw new Exception("User is not authenticated.");

        //        var order = await _context.Order.FirstOrDefaultAsync(o => o.Id == orderId);
        //        if (order == null) throw new Exception("Order not found.");
        //        if (order.Status != OrderStatus.ReadyForDispatch && order.Status != OrderStatus.Paid)
        //            throw new Exception("Order is not in a state to be dispatched.");

        //        // Get default sender address (your existing code)
        //        var senderAddress = await _context.Company
        //            .Where(a => a.Id == companyId)
        //            .Select(a => new AddressDto { name = a.Name, phone = a.PhoneNumber, email = a.Email, address = a.Address })
        //            .FirstOrDefaultAsync();

        //        var receiverAddress = new AddressDto
        //        {
        //            name = order.CustomerName,
        //            phone = order.WhatsAppNumber,
        //            email = order.CustomerEmail,
        //            address = order.DeliveryAddress
        //        };

        //        // --- SAFE: deserialize DB JSON into the DB-shaped class, then map to OrderItemDto ---
        //        var json = string.IsNullOrWhiteSpace(order.OrderItemsJson) ? "[]" : order.OrderItemsJson;

        //        // If your DB JSON matches OrderItem (Description, Price, Quantity), do this:
        //        var dbItems = JsonConvert.DeserializeObject<List<OrderItem>>(json) ?? new List<OrderItem>();

        //        // Map to the shipping DTO (fill defaults like weight/dimensions/category)
        //        var orderItemsForShipping = dbItems.Select(i => new OrderItemDto
        //        {
        //            ProductName = i.Description,      // fallback
        //            Description = i.Description,
        //            Price = i.Price,
        //            Quantity = i.Quantity,
        //            Weight = 1,                       // set sensible defaults or compute if you can
        //            PackageLength = 12,
        //            PackageWidth = 10,
        //            PackageHeight = 10,
        //            CategoryId = 1                     // or logic to derive category
        //        }).ToList();

        //        if (!orderItemsForShipping.Any()) throw new Exception("Order has no items.");

        //        // totals (optional)
        //        var totalWeight = orderItemsForShipping.Sum(x => x.Weight * x.Quantity);
        //        var totalAmount = orderItemsForShipping.Sum(x => x.Price * x.Quantity);

        //        // Build FetchRatesDto and attach Items — everything shipping needs is here
        //        var fetchRates = new FetchRatesDto
        //        {
        //            Sender = senderAddress,
        //            Receiver = receiverAddress,
        //            Weight = (decimal)totalWeight,
        //            Amount = totalAmount,
        //            ServiceType = "delivery",
        //            Items = orderItemsForShipping
        //        };

        //        // optional: set package dimension on DTO if you want
        //        fetchRates.PackageDimension = new PackageDimension
        //        {
        //            length = orderItemsForShipping.Max(i => i.PackageLength),
        //            width = orderItemsForShipping.Max(i => i.PackageWidth),
        //            height = orderItemsForShipping.Max(i => i.PackageHeight)
        //        };

        //        // Call the shipping service which will call FetchRatesAsync(fetchRates) internally
        //        var shipmentResult = await _shipBubbleService.CreateShipmentAutomaticallyAsync(fetchRates);

        //        // Update order with tracking info (your existing code)
        //        order.Status = OrderStatus.InTransit;
        //        order.TrackingNumber = shipmentResult.TrackingNumber;
        //        order.LogisticsPartner = shipmentResult.CourierName;
        //        order.UpdatedAt = DateTime.UtcNow;

        //        await _unitOfWork.SaveChangesAsync();

        //        return new BookDispatchResponseDto
        //        {
        //            OrderId = order.Id,
        //            TrackingNumber = order.TrackingNumber,
        //            LogisticsPartner = order.LogisticsPartner,
        //            NewStatus = order.Status
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"Error Dispatching Order: {ex.Message}");
        //    }
        //}
        public async Task<string> GetCompanyNameAsync()
        {
            try
            {
                if (!Guid.TryParse(_authenticatedUserService.CompanyId, out var companyId))
                {
                    throw new Exception("User is not authenticated.");
                }
                var company = await _context.Company
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == companyId);
                if (company == null)
                {
                    throw new Exception("Company not found.");
                }
                return company.Name;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving company name: {ex.Message}");
            }
        }

        public async Task<ProfileDetailsDto> GetProfileAsync()
        {
            try
            {
                if (!Guid.TryParse(_authenticatedUserService.CompanyId, out var companyId))
                {
                    throw new Exception("User is not authenticated.");
                }
                var company = await _context.Company
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == companyId);
                if (company == null)
                {
                    throw new Exception("Company not found.");
                }
                var profile = new ProfileDetailsDto
                {
                    StoreName = company.Name,
                    ContactEmail = company.Email,
                    PhoneNumber = company.PhoneNumber,
                    PrimaryAddress = company.Address
                };
                return profile;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving profile details: {ex.Message}");
            }

        }
        public async Task<Unit> UpdateProfileAsync(UpdateProfileCommand request)
        {
            try
            {
                if (!Guid.TryParse(_authenticatedUserService.CompanyId, out var companyId))
                {
                    throw new Exception("User is not authenticated.");
                }
                var company = await _context.Company
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == companyId);
                if (company == null)
                {
                    throw new Exception("Company not found.");
                }
                company.Email = request.ContactEmail;
                company.PhoneNumber = request.PhoneNumber;
                company.Address = request.PrimaryAddress;
                company.UpdatedAt = DateTime.UtcNow;
                _context.Company.Update(company);
                await _unitOfWork.SaveChangesAsync();
                return Unit.Value;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating profile details: {ex.Message}");
            }
        }

        public async Task<Response<string>> ForgotPasswordAsync(ForgotPasswordCommand request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                return Response<string>.Success("If an account exists for this email, a password reset link has been sent.");
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var tokenEncoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var jobId = _jobClient.Enqueue(() => SendPasswordResetEmail(
               user.Email,
              tokenEncoded,
              request.BaseUrl)
      );


            if (!string.IsNullOrEmpty(jobId))
            {

                _logger.LogInformation("Enqueued password reset email job {JobId} for user {Email}", jobId, user.Email);
            }
            return Response<string>.Success("If an account exists for this email, a password reset link has been sent.");
        }
        public async Task<Response<string>> SendPasswordResetEmail(string email, string tokenEncoded, string baseUrl)
        {
            if (_mailService == null)
            {

                throw new Exception("Mail service failed to inject into Hangfire job.");
            }
            var emailEncoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(email));
            var resetLink = $"{baseUrl}/reset-password?token={tokenEncoded}&email={emailEncoded}";
            var emailBody = $"<p>You requested a password reset. Please use the following link to reset your password:</p><p><a href='{resetLink}'>Reset Password</a></p>";
            await _mailService.SendEmailAsync(email, "SmartSeller Password Reset", emailBody);
            return Response<string>.Success("Password reset email sent successfully.");
        }
        public async Task<Response<string>> ResetPasswordAsync(ResetPasswordCommand request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);

                if (user == null)
                {
                    return Response<string>.Success("If the email exists, a reset link has been sent.");
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                var resetUrl = $"{_appSettings.FrontendUrl}/reset-password?email={user.Email}&token={encodedToken}";

                var emailBody = $@"
            <h3>Password Reset</h3>
            <p>Use the token below to reset your password:</p>
            <h2>{encodedToken}</h2>
            <p>Or click the link below:</p>
            <a href='{resetUrl}'>Reset Password</a>
        ";

                // 4. Send email
                await _mailService.SendEmailAsync(user.Email!, "Password Reset", emailBody);

                return Response<string>.Success("Password reset token sent to your email.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error sending reset token: {ex.Message}");
            }
        }


        public async Task<Response<List<BankDto>>> GetNigerianBanksAsync()
        {
            try
            {
                var banks = await _paystackService.GetNigerianBanksAsync();
                return Response<List<BankDto>>.Success(banks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching Nigerian banks from Paystack.");
                return Response<List<BankDto>>.Failure("Failed to retrieve Nigerian banks.");
            }
        }
        public async Task<Response<AccountVerificationResponseDto>> AccountVerification(VerifyBankAccountCommand request)
        {
            try
            {

                var (isSuccess, accountName, message) = await _paystackService.ResolveAccountAsync(request.AccountNumber, request.BankCode);
                if (!isSuccess)
                {
                    return Response<AccountVerificationResponseDto>.Failure(message);
                }

                var verificationDto = new AccountVerificationResponseDto
                {
                    Success = isSuccess,
                    AccountName = accountName,
                    Message = message
                };
                return Response<AccountVerificationResponseDto>.Success(verificationDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while verifying bank account {AccountNumber} with bank code {BankCode}.", request.AccountNumber, request.BankCode);
                return Response<AccountVerificationResponseDto>.Failure("Bank account verification failed due to internal error.");
            }
        }

        //public async Task<Response<List<ShipbubbleRateOption>>> GetRates(Guid orderId)
        //{
        //    try
        //    {
        //        if (!Guid.TryParse(_authenticatedUserService.CompanyId, out var companyId)) throw new Exception("User is not authenticated.");

        //        var order = await _context.Order.FirstOrDefaultAsync(o => o.Id == orderId);
        //        if (order == null) throw new Exception("Order not found.");
        //        if (order.Status != OrderStatus.ReadyForDispatch && order.Status != OrderStatus.Paid)
        //            throw new Exception("Order is not in a state to be dispatched.");

        //        // Get default sender address (your existing code)
        //        var senderAddress = await _context.Company
        //            .Where(a => a.Id == companyId)
        //            .Select(a => new AddressDto { name = a.Name, phone = a.PhoneNumber, email = a.Email, address = a.Address })
        //            .FirstOrDefaultAsync();

        //        var receiverAddress = new AddressDto
        //        {
        //            name = order.CustomerName,
        //            phone = order.WhatsAppNumber,
        //            email = order.CustomerEmail,
        //            address = order.DeliveryAddress
        //        };

        //        // --- SAFE: deserialize DB JSON into the DB-shaped class, then map to OrderItemDto ---
        //        var json = string.IsNullOrWhiteSpace(order.OrderItemsJson) ? "[]" : order.OrderItemsJson;

        //        // If your DB JSON matches OrderItem (Description, Price, Quantity), do this:
        //        var dbItems = JsonConvert.DeserializeObject<List<OrderItem>>(json) ?? new List<OrderItem>();

        //        // Map to the shipping DTO (fill defaults like weight/dimensions/category)
        //        var orderItemsForShipping = dbItems.Select(i => new OrderItemDto
        //        {
        //            ProductName = i.Description,      // fallback
        //            Description = i.Description,
        //            Price = i.Price,
        //            Quantity = i.Quantity,
        //            Weight = 1,                       // set sensible defaults or compute if you can
        //            PackageLength = 12,
        //            PackageWidth = 10,
        //            PackageHeight = 10,
        //            CategoryId = 1                     // or logic to derive category
        //        }).ToList();

        //        if (!orderItemsForShipping.Any()) throw new Exception("Order has no items.");

        //        // totals (optional)
        //        var totalWeight = orderItemsForShipping.Sum(x => x.Weight * x.Quantity);
        //        var totalAmount = orderItemsForShipping.Sum(x => x.Price * x.Quantity);

        //        // Build FetchRatesDto and attach Items — everything shipping needs is here
        //        var fetchRates = new FetchRatesDto
        //        {
        //            Sender = senderAddress,
        //            Receiver = receiverAddress,
        //            Weight = (decimal)totalWeight,
        //            Amount = totalAmount,
        //            ServiceType = "delivery",
        //            Items = orderItemsForShipping
        //        };

        //        // optional: set package dimension on DTO if you want
        //        fetchRates.PackageDimension = new PackageDimension
        //        {
        //            length = orderItemsForShipping.Max(i => i.PackageLength),
        //            width = orderItemsForShipping.Max(i => i.PackageWidth),
        //            height = orderItemsForShipping.Max(i => i.PackageHeight)
        //        };
        //        dynamic rawResult = await _shipBubbleService.FetchRatesAsync(fetchRates);

        //        var apiResponse = ((Newtonsoft.Json.Linq.JObject)rawResult).ToObject<ShipbubbleApiResponse>();

        //        if (apiResponse?.Data?.Couriers == null)
        //        {
        //            return new Response<List<ShipbubbleRateOption>>("No shipping options available.");
        //        }

        //        // 4. Map the API Data to YOUR App's DTO
        //        var mappedRates = apiResponse.Data.Couriers.Select(c => new ShipbubbleRateOption
        //        {
        //            RateId = c.ServiceCode,
        //            CourierName = c.CourierName,
        //            ServiceName = c.ServiceType,
        //            Price = c.Total,
        //            Currency = c.Currency,
        //            EstimatedDeliveryTime = c.DeliveryEta,
        //            CourierLogoUrl = c.CourierImage
        //        }).ToList();

        //        return new Response<List<ShipbubbleRateOption>>(mappedRates);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"Error Getting Order: {ex.Message}");
        //    }
        //}

        public async Task<Response<string>> AddCustomerAsync(Customerrequest request)
        {
            if (!Guid.TryParse(_authenticatedUserService.CompanyId, out var companyId))
            {
                throw new Exception("User is not authenticated.");
            }
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                Name = request.Name,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                WhatsAppNumber = request.WhatsAppNumber,
                Address = request.Address,
                CreatedAt = DateTime.UtcNow
            };
            await _context.Customers.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync();
            return Response<string>.Success("Customer added successfully.");
        }
        
    public async Task<Response<string>> ChangePasswordAsync(ChangePasswordCommand request)
        {
            try
            {
                if (request.NewPassword != request.ConfirmPassword)
                {
                    return Response<string>.Failure("Passwords do not match.");
                }

                var decodedToken = Encoding.UTF8.GetString(
                    WebEncoders.Base64UrlDecode(request.Token)
                );

                var users = _userManager.Users.ToList();

                foreach (var user in users)
                {
                    var result = await _userManager.ResetPasswordAsync(
                        user,
                        decodedToken,
                        request.NewPassword
                    );

                    if (result.Succeeded)
                    {
                        return Response<string>.Success("Password changed successfully.");
                    }
                }

                return Response<string>.Failure("Invalid or expired token.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error changing password: {ex.Message}");
            }
        }
    }
}




