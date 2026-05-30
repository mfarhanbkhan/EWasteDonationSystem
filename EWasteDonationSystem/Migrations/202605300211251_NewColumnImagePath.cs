namespace EWasteDonationSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NewColumnImagePath : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StudentApplications", "ImagePath", c => c.String(maxLength: 260));
        }
        
        public override void Down()
        {
            DropColumn("dbo.StudentApplications", "ImagePath");
        }
    }
}
