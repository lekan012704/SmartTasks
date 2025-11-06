using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Dto.Account
{
    public class UserDto
    {
        public required string Id { get; set; }
        public required string Email { get; set; }
        public required string UserName { get; set; }
        public required string Role { get; set; }
        public required string PhoneNumber { get; set; }
        public  bool? IsActive { get; set; }
        public  DateTime? DateCreated { get; set; }
        public  string? CreatedBy { get; set; }
        public  string? FullName { get; set; }
    }
}
