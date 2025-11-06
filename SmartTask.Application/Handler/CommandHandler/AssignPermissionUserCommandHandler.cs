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
    public class AssignPermissionUserCommandHandler : IRequestHandler<AssignPermissionUserCommand, Response<List<string>>>
    {
        private readonly IRoleService _entityMananger;

        public AssignPermissionUserCommandHandler(IRoleService entityMananger)
        {
            _entityMananger = entityMananger;
        }
        public async Task<Response<List<string>>> Handle(AssignPermissionUserCommand request,CancellationToken cancellationToken)
        {
            return await _entityMananger.AddPermissionUserAsync(request.Permission);
        }
    }
   
}
