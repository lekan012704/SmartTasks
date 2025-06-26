using MediatR;
using SmartTask.Application.Command;
using SmartTask.Application.Interfaces;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Handler.CommandHandler
{
    public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Response<string>>
    {
       private readonly IRoleService _roleService;
        public CreateRoleCommandHandler(IRoleService roleService)
        {
            _roleService = roleService;
        }

        public async Task<Response<string>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
        {
           return await _roleService.CreateRoleAsync(request.RoleModel);
        }
    }

}
