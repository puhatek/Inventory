using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using Inventory.Models.Inflows;

namespace Inventory.Models
{
    public class MagazynDbContext: DbContext
    {

        public MagazynDbContext() : base("MagazynDbContext") { }
   
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        
        public DbSet<Inflow> Inflows { get; set; }

        public DbSet<InflowItem> InflowItems { get; set; }

        //protected override void OnModelCreating(DbModelBuilder modelBuilder)
        //{
        //
        //    modelBuilder.Entity<OrderItem>()
        //        .HasKey(t => new { t.AssetId, t.OrderId })
        //        .ToTable("OrderItems");
        //
        //    modelBuilder.Entity<InflowItem>()
        //        .ToTable("InflowItems");
        //        //.HasRequired(t => t.Orders)
        //        //.WithMany()
        //        //.HasForeignKey(t => new { t.AssetId, t.OrderId, t.InflowId });
        //           
        //    modelBuilder.Entity<Asset>().ToTable("Assets");
        //        
        //    base.OnModelCreating(modelBuilder);
        //}

    }

    
}