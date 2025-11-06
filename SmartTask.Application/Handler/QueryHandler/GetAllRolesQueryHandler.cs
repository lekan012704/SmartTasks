using MediatR;
using SmartTask.Application.Dto.Role;
using SmartTask.Application.Interfaces;
using SmartTask.Application.Query;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Handler.QueryHandler
{
    public class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, Response<List<RoleDto>>>
    {

        private readonly IRoleService _roleService;
        public GetAllRolesQueryHandler(IRoleService roleService)
        {
            _roleService = roleService;
        }

        public async Task<Response<List<RoleDto>>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
        {
            return await _roleService.GetAllRolesAsync();
           
        }
    }
}
