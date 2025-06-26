using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Dto.Role
{
    public class PermissionDto
    {
        public string RoleName { get; set; }
        public List<string> Permissions { get; set; } 
    }
}
