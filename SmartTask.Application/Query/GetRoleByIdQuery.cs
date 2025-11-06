using MediatR;
using SmartTask.Application.Dto.Role;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Query
{
    public class GetRoleByIdQuery : IRequest<Response<RoleDto>>
    {
        public string RoleId { get; set; }

        public GetRoleByIdQuery(string roleId)
        {
            RoleId = roleId;
        }
    }

}
