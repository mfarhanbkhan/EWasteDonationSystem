namespace EWasteDonationSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FullName = c.String(nullable: false, maxLength: 80),
                        Email = c.String(maxLength: 120),
                        Password = c.String(nullable: false, maxLength: 200),
                        RoleType = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAtUtc = c.DateTime(nullable: false),
                        EmailOtp = c.String(maxLength: 200),
                        OtpExpiresAt = c.DateTime(),
                        IsEmailVerified = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Users");
        }
    }
}
