using MediatR;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Query
{

    public class GetUsersInRoleQuery : IRequest<Response<List<string>>>
    {
        public string RoleName { get; set; }

        public GetUsersInRoleQuery(string roleName)
        {
            RoleName = roleName;
        }
    }
}
