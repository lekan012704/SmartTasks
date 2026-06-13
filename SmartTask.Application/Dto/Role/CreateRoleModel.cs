using System;
using System.Collections.Generic;

namespace SmartTask.Application.Dto.Role
{
    public class CreateRoleModel
    {
        public string RoleName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public List<string>? Claims { get; set; } = new();
        public string? CompanyId { get; set; } 
    }
}