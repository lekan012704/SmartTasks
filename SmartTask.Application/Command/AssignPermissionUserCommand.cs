using MediatR;
using SmartTask.Application.Dto.Role;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Command
{
    public class AssignPermissionUserCommand : IRequest<Response<List<string>>>
    {
        public AssignUserPermissionsDto Permission {  get; set; }
        public AssignPermissionUserCommand(AssignUserPermissionsDto permission)
        {
            Permission = permission;
        }
    }
}
