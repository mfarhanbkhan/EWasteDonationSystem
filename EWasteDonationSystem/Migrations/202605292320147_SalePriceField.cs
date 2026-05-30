namespace EWasteDonationSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SalePriceField : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DonationItems", "SalePrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.DonationItems", "SalePrice");
        }
    }
}
