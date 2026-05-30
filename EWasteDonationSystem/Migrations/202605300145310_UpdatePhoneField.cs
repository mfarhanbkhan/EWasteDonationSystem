namespace EWasteDonationSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatePhoneField : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Donors", "UserName", c => c.String(maxLength: 30));
            DropColumn("dbo.Donors", "Phone");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Donors", "Phone", c => c.String(maxLength: 30));
            DropColumn("dbo.Donors", "UserName");
        }
    }
}
