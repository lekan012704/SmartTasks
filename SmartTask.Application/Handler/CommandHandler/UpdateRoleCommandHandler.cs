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
    public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, Response<string>>
    {
        private readonly IRoleService _roleService;

        public UpdateRoleCommandHandler(IRoleService roleService)
        {
            _roleService = roleService;
        }

        public async Task<Response<string>> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
        {
            return await _roleService.UpdateRoleAsync(request);
        }
    }
}
