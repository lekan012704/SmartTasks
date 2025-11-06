using SmartTask.Application.Command;
using SmartTask.Application.Command.Task;
using SmartTask.Application.Dto;
using SmartTask.Application.Dto.Account;
using SmartTask.Application.Dto.Role;
using SmartTask.Application.Query;
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
       
        Task<Response<List<CompanyTypeDto>>> GetAllCompanyTypeAsync();
        Task<Response<List<UserDto>>> GetUsersByCompanyAsync(GetUsersByCompany request);
        Task<Response<List<string>>> AddPermissionsToRoleAsync(PermissionDto request);
        Task<Response<string>> UpdateUserAsync(string Id, UpdateUserRequestDto requestDto);
        Task<Response<string>> DeleteUserAsync(string userId);
        Task<Response<string>> DeactivateUserAsync(string userId);
        Task<Response<string>> ActivateUserAsync(string userId);
    

    }
}
 