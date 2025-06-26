using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartTask.Application.Interfaces;
using SmartTask.Domain.Constants;
using SmartTask.Domain.Entities;
using SmartTask.Identity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Persistence.Contexts
{
    public partial class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IDateTimeService _dateTime;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
     : base(options)
        {
        }

        // Existing constructor with IDateTimeService
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IDateTimeService dateTime)
            : base(options)
        {
            _dateTime = dateTime;
        }


        public virtual DbSet<Company> Company { get; set; }
        public virtual DbSet<TaskItem> TaskItem { get; set; }
        public virtual DbSet<AuditLog> AuditLog { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // Make sure to call this first

            // Configure relationship here:
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Company)
                .WithMany()
                .HasForeignKey(u => u.CompanyId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
            builder.Entity<TaskItem>()
    .Property(t => t.Priority)
    .HasConversion<string>();

            builder.Entity<TaskItem>()
                .Property(t => t.Status)
                .HasConversion<string>();
            builder.Entity<TaskItem>()
        .HasOne<ApplicationUser>() // No navigation in TaskItem
        .WithMany(u => u.AssignedTasks) // Optional navigation
        .HasForeignKey(t => t.AssignedUserId)
        .OnDelete(DeleteBehavior.Restrict);
        }
    }

}
