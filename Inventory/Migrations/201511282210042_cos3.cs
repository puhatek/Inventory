namespace Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class cos3 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.OrderItems", "Value", c => c.Single(nullable: false));
        }
        
        public override void Down()
        {
            //AlterColumn("dbo.OrderItems", "Value", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}
