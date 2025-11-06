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
    public class AssignPermissionsToRoleCommandHandler : IRequestHandler<AssignPermissionsToRoleCommand, Response<List<string>>>
    {
        private readonly IEntityManagerAsync _entityManager;

        public AssignPermissionsToRoleCommandHandler(IEntityManagerAsync entityManager)
        {
            _entityManager = entityManager;
        }
        public async Task<Response<List<string>>> Handle(AssignPermissionsToRoleCommand request,CancellationToken cancellationToken)
        {
            return await _entityManager.AddPermissionsToRoleAsync(request._permission);
        }
    }
}
