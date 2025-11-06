using MediatR;
using SmartTask.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Command
{
    public class AddClaimsToRoleCommand : IRequest<Response<string>>
    {
        public string RoleId { get; set; }
        public List<string> Claims { get; set; }
    }
}
