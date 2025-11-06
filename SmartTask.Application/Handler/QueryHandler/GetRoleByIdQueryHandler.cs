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
    public class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, Response<RoleDto>>
    {

        private readonly IRoleService _roleService;
        public GetRoleByIdQueryHandler(IRoleService roleService)
        {
            _roleService = roleService;
        }

        public async Task<Response<RoleDto>> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
        {
            return await _roleService.GetRoleByIdAsync(request.RoleId);

        }
    
    }
}
