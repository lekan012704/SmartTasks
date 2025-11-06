using Microsoft.AspNetCore.Identity;
using SmartTask.Application.Enums;
using SmartTask.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Domain.Models
{
    public class ApplicationUser : IdentityUser
    {
        public Guid? CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? TaxIdentificationNumber { get; set; }
        public string? ContactEmail { get; set; }
        public string? FullName { get; set; }
        public string? ContactPhone { get; set; }
        public string? CompanyAddress { get; set; }
        public CompanyType Type { get; set; }
        public Company Company { get; set; }    
        public bool? IsActive { get; set; }
        public List<RefreshToken> RefreshTokens { get; set; }
        public ICollection<Order> AssignedTasks { get; set; }
        public string? DisabledBy { get; set; }
        public string? EnabledBy { get; set; }
        public DateTime? DateEnabled { get; set; }
        public DateTime? DateDisabled { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }
        public DateTime? DateCreated { get; set; } = DateTime.Now;
        public string? DeletedBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string? UpdatedBy { get; set; }

        public bool OwnsToken(string token)
        {
            return this.RefreshTokens?.Find(x => x.Token == token) != null;
        }
    }
}
