namespace Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class cos2 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.OrderItems", "Value", c => c.Single(nullable: false));
        }
        
        public override void Down()
        {
            //AlterColumn("dbo.OrderItems", "Value", c => c.Single(nullable: false));
        }
    }
}
