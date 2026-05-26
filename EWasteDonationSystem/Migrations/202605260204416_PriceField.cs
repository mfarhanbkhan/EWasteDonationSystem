namespace EWasteDonationSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PriceField : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DonationItems", "Price", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.DonationItems", "Price");
        }
    }
}
