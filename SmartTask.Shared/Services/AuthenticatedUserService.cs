using Microsoft.AspNetCore.Http;
using SmartTask.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Shared.Services
{
    public class AuthenticatedUserService : IAuthenticatedUserService
    {
        public AuthenticatedUserService(IHttpContextAccessor httpContextAccessor)   
        {
            UserId=httpContextAccessor.HttpContext?.User?.FindFirst("UserId")?.Value;
            Email = httpContextAccessor.HttpContext?.User?.FindFirst("Email")?.Value;
            UserName = httpContextAccessor.HttpContext?.User?.FindFirst("UserName")?.Value;
            CompanyId = httpContextAccessor.HttpContext?.User?.FindFirst("CompanyId")?.Value;
            CompanyName = httpContextAccessor.HttpContext?.User?.FindFirst("CompanyName")?.Value;
            Role = httpContextAccessor.HttpContext?.User?.FindFirst("Role")?.Value;
            RoleName = httpContextAccessor.HttpContext?.User?.FindFirst("RoleName")?.Value;
        }
        public string UserId { get; }
        public string Email { get; }
        public string UserName { get; }
        public string CompanyId { get; }
        public string CompanyName { get; }
        public string Role { get; }
        public string RoleName { get; }
    }
}
