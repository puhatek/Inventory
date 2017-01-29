namespace Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class cos : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.OrderItems", "Value", c => c.Single(nullable: false));
            AddColumn("dbo.OrderItems", "Vat", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.OrderItems", "Qty", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropColumn("dbo.OrderItems", "Price");
        }
        
        public override void Down()
        {
            //AddColumn("dbo.OrderItems", "Price", c => c.Single(nullable: false));
            //AlterColumn("dbo.OrderItems", "Qty", c => c.Int(nullable: false));
            //DropColumn("dbo.OrderItems", "Vat");
            //DropColumn("dbo.OrderItems", "Value");
        }
    }
}
