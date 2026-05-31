using MediatR;
using SmartTask.Application.Command;
using SmartTask.Application.Command.Customer;
using SmartTask.Application.Command.Order;
using SmartTask.Application.Dto;
using SmartTask.Application.Dto.Account;
using SmartTask.Application.Dto.Customer;
using SmartTask.Application.Dto.Logistics;
using SmartTask.Application.Dto.Order;
using SmartTask.Application.Dto.Paystack;
using SmartTask.Application.Dto.Role;
using SmartTask.Application.Features.Orders.Commands;
using SmartTask.Application.Features.Orders.Queries;
using SmartTask.Application.Query;
using SmartTask.Application.Query.Customer;
using SmartTask.Application.Query.Logistics;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Interfaces
{
    public interface IEntityManagerAsync
    {
        Task<Response<CompanyResponse>> RegisterCompanyAsync(CompanyRequest request);
        Task<Response<List<string>>> AddPermissionAsync(PermissionDto request);
        Task<Response<UserResponseDto>> RegisterUserAsync(UserRequestDto request);
        Task<Response<string>> AddCustomerAsync(Customerrequest request);
        Task<Response<List<CompanyTypeDto>>> GetAllCompanyTypeAsync();
        Task<Response<List<UserDto>>> GetUsersByCompanyAsync(GetUsersByCompany request);
        Task<Response<List<string>>> AddPermissionsToRoleAsync(PermissionDto request);
        Task<Response<string>> UpdateUserAsync(string Id, UpdateUserRequestDto requestDto);
        Task<Response<string>> DeleteUserAsync(string userId);
        Task<Response<string>> DeactivateUserAsync(string userId);
        Task<Response<string>> ActivateUserAsync(string userId);
        Task<Guid> CreateOrderAsync(CreateOrderCommand request);
        Task<Unit> FulfillBatchManuallyAsync(FulfillBatchManuallyCommand request);
        Task<List<OrderSummaryDto>> GetAllOrderAsync(GetAllOrdersQuery request);
        Task<OrderDto> GetOrderByIdAsync(GetOrderByIdQuery request);
        Task<CustomerDto> GetCustomerByIdAsync(GetCustomerByIdQuery request);
        Task<Unit> UpdateStatusAsync(UpdateOrderStatusCommand request);
        Task<Unit> UpdateCustomerAsync(UpdateCustomerCommand request);
        Task<DashboardStatsDto> GetDasboardAsync();
        Task<List<CustomerDto>> GetAllCustomersAsync();
        Task<Unit> DeleteOrderAsync(DeleteOrderCommand request);
        Task<Unit> DeleteCustomerAsync(DeleteCustomerCommand request);
         //Task<BookDispatchResponseDto> DispatchOrderAsync(Guid orderId);
        Task<string> GetCompanyNameAsync();
        Task<ProfileDetailsDto> GetProfileAsync();
        Task<Unit> UpdateProfileAsync(UpdateProfileCommand request);
        Task<Response<string>> ForgotPasswordAsync(ForgotPasswordCommand request);
        Task<Response<string>> ResetPasswordAsync(ResetPasswordCommand request);
        Task<Response<string>> ChangePasswordAsync(ChangePasswordCommand request);
        Task<Response<List<BankDto>>> GetNigerianBanksAsync();
        Task<Response<AccountVerificationResponseDto>> AccountVerification(VerifyBankAccountCommand request);
        //Task<Response<List<ShipbubbleRateOption>>> GetRates(Guid orderId);
    }
}
 