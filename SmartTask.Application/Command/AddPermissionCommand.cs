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
    public class AddPermissionCommand  :IRequest<Response<List<string>>>
    {
      public PermissionDto Permission { get; set; }
        public AddPermissionCommand(PermissionDto permissionDto)
        {
          Permission = permissionDto;
        }
    }
}
