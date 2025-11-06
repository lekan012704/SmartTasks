using MediatR;
using SmartTask.Application.Dto.Role;
using SmartTask.Application.Dto.Task;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Command
{
    public class AssignPermissionsToRoleCommand : IRequest<Response<List<string>>>
    {
        public PermissionDto _permission {  get; set; }
        public AssignPermissionsToRoleCommand(PermissionDto permission)
        {
            _permission = permission;
        }
    }
}
