using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartTask.Application.Dto.Account;
using SmartTask.Application.Interfaces;
using SmartTask.Domain.Constants;
using SmartTask.Domain.Entities;
using SmartTask.Domain.Models;
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
        public virtual DbSet<Project> Project { get; set; }
        public virtual DbSet<ProjectMember> ProjectMember { get; set; }
        public virtual DbSet<Sprint> Sprint { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Permission> Permission { get; set; }
        public DbSet<RolePermission> RolePermission { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); 

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
        .HasOne<ApplicationUser>() 
        .WithMany(u => u.AssignedTasks) 
        .HasForeignKey(t => t.AssignedUserId)
        .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(rp => new { rp.RoleId, rp.PermissionId });

                entity.HasOne(rp => rp.Role)
                      .WithMany() // Or .WithMany(r => r.RolePermissions) if you add navigation
                      .HasForeignKey(rp => rp.RoleId)
                      .HasConstraintName("FK_RolePermission_Roles_RoleId")
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(rp => rp.Permission)
                      .WithMany(p => p.RolePermission)
                      .HasForeignKey(rp => rp.PermissionId)
                      .HasConstraintName("FK_RolePermission_Permissions_PermissionId")
                      .OnDelete(DeleteBehavior.Cascade);
            });
            builder.Entity<Project>(b =>
            {
                b.ToTable("Project");
                b.HasKey(p => p.ProjectId);
                b.Property(p => p.ProjectName)
                    .IsRequired()
                    .HasMaxLength(150);

                b.Property(p => p.Slug)
                    .HasMaxLength(200);

                b.Property(p => p.Description)
                    .HasMaxLength(1000);

                b.Property(p => p.Status)
                    .IsRequired();

                b.Property(p => p.Visibility)
                    .IsRequired();

                b.Property(p => p.CompanyId)
                    .IsRequired();

                b.Property(p => p.TotalTasks)
                    .HasDefaultValue(0);

                b.Property(p => p.OpenTasks)
                    .HasDefaultValue(0);

                b.Property(p => p.IsDeleted)
                    .HasDefaultValue(false);

                b.Property(p => p.IsArchived)
                    .HasDefaultValue(false);

                b.Property(p => p.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                b.Property(p => p.CreatedBy)
                    .HasMaxLength(100);

                b.Property(p => p.UpdatedBy)
                    .HasMaxLength(100);

                b.HasOne(p => p.ProjectLead)
                    .WithMany() 
                    .HasForeignKey(p => p.ProjectLeadId)
                    .OnDelete(DeleteBehavior.SetNull);

                b.HasMany(p => p.Members)
                    .WithOne(m => m.Project)
                    .HasForeignKey(m => m.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasMany(p => p.Tasks)
                    .WithOne(t => t.Project)
                    .HasForeignKey(t => t.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasMany(p => p.Sprints)
                    .WithOne(s => s.Project)
                    .HasForeignKey(s => s.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            builder.Entity<Project>()
               .Property(t => t.Status)
               .HasConversion<string>();
            builder.Entity<Project>()
               .Property(t => t.Visibility)
               .HasConversion<string>();
            builder.Entity<ProjectMember>(b =>
            {
                b.HasKey(pm => pm.Id);

                b.HasOne(pm => pm.User)
                    .WithMany() 
                    .HasForeignKey(pm => pm.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<TaskItem>(b =>
            {
                b.HasKey(t => t.Id);
                b.Property(t => t.Title).HasMaxLength(200);
                b.HasOne(t => t.Project)
                    .WithMany(p => p.Tasks)
                    .HasForeignKey(t => t.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Sprint>(b =>
            {
                b.HasKey(s => s.Id);
                b.Property(s => s.Name).HasMaxLength(200);
                b.HasOne(s => s.Project)
                    .WithMany(p => p.Sprints)
                    .HasForeignKey(s => s.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);
            });


        }
    }

}
