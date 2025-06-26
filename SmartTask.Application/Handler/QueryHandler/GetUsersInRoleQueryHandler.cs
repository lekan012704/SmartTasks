using MediatR;
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
    public class GetUsersInRoleQueryHandler : IRequestHandler<GetUsersInRoleQuery, Response<List<string>>>
    {
        private readonly IRoleService _roleService;

        public GetUsersInRoleQueryHandler(IRoleService roleService)
        {
            _roleService = roleService;
        }

        public async Task<Response<List<string>>> Handle(GetUsersInRoleQuery request, CancellationToken cancellationToken)
        {
            return await _roleService.GetUsersInRoleAsync(request.RoleName);
        }
    }
}
