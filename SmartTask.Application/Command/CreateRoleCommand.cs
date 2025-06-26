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
    public class CreateRoleCommand : IRequest<Response<string>>
    {
        public CreateRoleModel RoleModel { get; set; }

        public CreateRoleCommand(CreateRoleModel roleModel)
        {
            RoleModel = roleModel;
        }
    }


}
