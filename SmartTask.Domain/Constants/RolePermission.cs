using Microsoft.AspNetCore.Identity;
using SmartTask.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Domain.Constants
{
    public class RolePermission
    {
        public string RoleId { get; set; }
        public IdentityRole Role { get; set; }
        public Guid PermissionId { get; set; }
        public Permission Permission { get; set; }
    }
}
