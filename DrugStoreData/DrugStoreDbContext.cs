using DrugStoreCore.Enums;
using DrugStoreData.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DrugStore.Data
{
    public class DrugStoreDbContext : IdentityDbContext<Pharmacy>
    {
        public DrugStoreDbContext(DbContextOptions<DrugStoreDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<OrderDetails>()
                        .Property(ord => ord.Status)
                        .HasDefaultValue(Status.Pending);
            builder.Entity<Pharmacy>()
                        .Property(phar => phar.PharmacyStatus)
                        .HasDefaultValue(PharmacyStatus.Active);
        }
        public DbSet<Drug> Drugs { get; set; }
        public DbSet<OrderDetails> OrdersDetails { get; set; }
        public DbSet<PharmacyOrders> PharmacysOrders { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<PharmacyNotes> PharmacyNotes { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NewConact> NewConacts { get; set; }
        public DbSet<SpecialCompany> SpecialCompanies { get; set; }
        public DbSet<SpecialLocations> SpecialLocations { get; set; }
    }
}
