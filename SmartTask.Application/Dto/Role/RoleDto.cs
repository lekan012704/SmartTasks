using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Dto.Role
{
    public class RoleDto
    {
        public string Id { get; set; }
        public string RoleName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public List<string>? Claims { get; set; }
    }

}
