namespace EWasteDonationSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class StatusField : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DonationItems", "Status", c => c.Int(nullable: false));
            AddColumn("dbo.StudentApplications", "Status", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.StudentApplications", "Status");
            DropColumn("dbo.DonationItems", "Status");
        }
    }
}
