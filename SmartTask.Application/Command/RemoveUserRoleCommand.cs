using MediatR;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Command
{

    public class RemoveUserRoleCommand : IRequest<Response<string>>
    {
        public string UserId { get; set; }
        public string RoleName { get; set; }

        public RemoveUserRoleCommand(string userId, string roleName)
        {
            UserId = userId;
            RoleName = roleName;
        }
    }
}
