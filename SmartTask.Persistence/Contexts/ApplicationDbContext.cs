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
using Microsoft.AspNetCore.Identity; 

namespace SmartTask.Persistence.Contexts
{
    public partial class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IDateTimeService _dateTime;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
       : base(options)
        {
        }


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IDateTimeService dateTime)
            : base(options)
        {
            _dateTime = dateTime;
        }

        public virtual DbSet<Company> Company { get; set; }
        public virtual DbSet<AddressBook> AddressBook { get; set; }
        public virtual DbSet<Order> Order { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Permission> Permission { get; set; }   
        public DbSet<RolePermission> RolePermission { get; set; }
        public DbSet<Notification> Notification { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
           
            base.OnModelCreating(builder);
            builder.Entity<Notification>().HasNoKey();



            builder.Entity<ApplicationUser>(entity =>
            {
              
                entity.ToTable(name: "User");

                entity.HasOne(u => u.Company)
                      .WithMany()
                      .HasForeignKey(u => u.CompanyId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .IsRequired(false);
            });

            builder.Entity<IdentityRole>(entity => entity.ToTable(name: "Role"));
            builder.Entity<IdentityUserRole<string>>(entity => entity.ToTable("UserRole"));
            builder.Entity<IdentityUserClaim<string>>(entity => entity.ToTable("UserClaim"));
            builder.Entity<IdentityUserLogin<string>>(entity => entity.ToTable("UserLogin"));
            builder.Entity<IdentityRoleClaim<string>>(entity => entity.ToTable("RoleClaim"));
            builder.Entity<IdentityUserToken<string>>(entity => entity.ToTable("UserToken"));

            builder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(rp => new { rp.RoleId, rp.PermissionId });

                entity.HasOne(rp => rp.Role)
                      .WithMany()
                      .HasForeignKey(rp => rp.RoleId)
                      .HasConstraintName("FK_RolePermission_Roles_RoleId")
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(rp => rp.Permission)
                      .WithMany(p => p.RolePermission)
                      .HasForeignKey(rp => rp.PermissionId)
                      .HasConstraintName("FK_RolePermission_Permissions_PermissionId")
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Order>(entity =>
            {
               
                entity.HasOne(o => o.ApplicationUser)
                      .WithMany() 
                      .HasForeignKey(o => o.ApplicationUserId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Restrict); 

              
                entity.Property(o => o.CustomerName).HasMaxLength(255).IsRequired();
                entity.Property(o => o.WhatsAppNumber).HasMaxLength(50);
                entity.Property(o => o.DeliveryAddress).HasMaxLength(1000);
                entity.Property(o => o.LogisticsPartner).HasMaxLength(100);
                entity.Property(o => o.TrackingNumber).HasMaxLength(255);
                entity.Property(o => o.ManualRiderName).HasMaxLength(255);
                entity.Property(o => o.ManualTrackingInfo).HasMaxLength(255);

            
                entity.Property(o => o.Subtotal).HasColumnType("decimal(18, 2)");
                entity.Property(o => o.DeliveryFee).HasColumnType("decimal(18, 2)");
                entity.Property(o => o.TotalDue).HasColumnType("decimal(18, 2)");

                entity.HasIndex(o => o.ApplicationUserId);

                entity.HasQueryFilter(o => !o.IsDeleted);
            });
        }
    }
}