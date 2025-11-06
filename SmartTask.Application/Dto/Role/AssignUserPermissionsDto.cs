using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Dto.Role
{
    public class AssignUserPermissionsDto
    {
      
        public required string UserId { get; set; }

        public required List<string> Permissions { get; set; }
    }
}
