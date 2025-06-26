using SmartTask.Application.Command;
using SmartTask.Application.Dto.Role;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Interfaces
{
    public interface IRoleService
    {
        Task<bool> RoleExistsAsync(string roleName);

        Task<Response<string>> CreateRoleAsync(CreateRoleModel request);

        Task<Response<string>> AssignRoleToUserAsync(string userId, string roleName);

        Task<Response<List<RoleDto>>> GetAllRolesAsync();

        Task<Response<RoleDto?>> GetRoleByIdAsync(string id);

        Task<Response<string>> UpdateRoleAsync(UpdateRoleCommand request);

        Task<Response<string>> DeleteRoleAsync(string roleId);

        Task<Response<string>> RemoveRoleFromUserAsync(RemoveUserRoleCommand request);
            
        Task<Response<List<string>>> GetUsersInRoleAsync(string roleName);

        Task<Response<string>> AddClaimsToRoleAsync(string roleId, List<string> claims);
    }

}
