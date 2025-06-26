using MediatR;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Command
{
    public class DeleteRoleCommand : IRequest<Response<string>>
    {
        public string RoleId { get; set; }

        public DeleteRoleCommand(string roleId)
        {
            RoleId = roleId;
        }
    }
}
