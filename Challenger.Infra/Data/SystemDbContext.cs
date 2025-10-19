using Challenger.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Challenger.Infra.Data
{
    public class SystemDbContext:DbContext
    {
        public SystemDbContext(DbContextOptions<SystemDbContext> options):base(options)
        {
            
        }
        public DbSet<User> Users { get; private set;}
        public DbSet<Motorcycle> Motorcycles { get; private set; }
        public DbSet<Notification> Notifications { get; private set; }
        public DbSet<Rental> Rentals { get; private set; }
        public DbSet<DeliverymanProfile> DeliverymanProfiles { get; private set; }
        public DbSet<HighlightedMotorcycle> HighlightedMotorcycles { get; private set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Motorcycle>(entity =>
            {
                entity.HasIndex(m => m.Plate).IsUnique();
                entity.Property(m => m.Plate).IsRequired();
                entity.Property(m => m.Model).IsRequired();
                entity.Property(m => m.Identifier).IsRequired();
            });

            modelBuilder.Entity<DeliverymanProfile>(entity =>
            {
                entity.Property(d => d.CnhNumber).IsRequired();
                entity.Property(d => d.CnhType).IsRequired();
                entity.Property(d => d.Cnpj).IsRequired();
                entity.HasIndex(d => d.CnhNumber).IsUnique();
                entity.HasIndex(d => d.Cnpj).IsUnique();
            });

            modelBuilder.Entity<HighlightedMotorcycle>(entity =>
            {
                entity.ToTable("highlighted_motorcycles");
                entity.HasKey(h => h.Id);
                entity.HasIndex(h => h.MotorcycleId).IsUnique();
            });

            modelBuilder.Entity<Rental>(entity =>
            {
                // Optional: enforce start date is the next day after created_at at DB level (PostgreSQL)
                entity.ToTable("Rentals", tb =>
                    tb.HasCheckConstraint(
                        "CK_Rentals_StartDate_NextDay",
                        "\"StartDate\" = date_trunc('day', \"CreatedAt\") + interval '1 day'"));
            });
        }
    }
}
