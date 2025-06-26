using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartTask.Application.Dto.Account;
using SmartTask.Domain.Constants;
using SmartTask.Domain.Entities;
using SmartTask.Identity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Identity.Contexts
{
    public class IdentityContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public IdentityContext(DbContextOptions<IdentityContext> options) : base(options)
        {
        }

        // Optional: Add DbSet for RefreshTokens if you're using them
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Permission> Permission { get; set; }
        public DbSet<RolePermission> RolePermission { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Customize table names if needed
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("Users"); // Instead of AspNetUsers
            });

            builder.Entity<IdentityRole>(entity =>
            {
                entity.ToTable("Roles");
            });

            builder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.ToTable("UserRoles");
            });

            builder.Entity<IdentityUserClaim<string>>(entity =>
            {
                entity.ToTable("UserClaims");
            });

            builder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.ToTable("UserLogins");
            });

            builder.Entity<IdentityRoleClaim<string>>(entity =>
            {
                entity.ToTable("RoleClaims");
            });

            builder.Entity<IdentityUserToken<string>>(entity =>
            {
                entity.ToTable("UserTokens");
            });

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
        }
    }
}
