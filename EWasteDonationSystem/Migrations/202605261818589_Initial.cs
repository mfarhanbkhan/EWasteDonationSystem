namespace EWasteDonationSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ChatMessages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SenderRole = c.String(nullable: false, maxLength: 20),
                        Message = c.String(nullable: false, maxLength: 200),
                        SentAtUtc = c.DateTime(nullable: false),
                        DonorId = c.Int(),
                        StudentId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Donors", t => t.DonorId, cascadeDelete: true)
                .ForeignKey("dbo.Students", t => t.StudentId, cascadeDelete: true)
                .Index(t => t.DonorId)
                .Index(t => t.StudentId);
            
            CreateTable(
                "dbo.Donors",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FullName = c.String(nullable: false, maxLength: 80),
                        Phone = c.String(maxLength: 30),
                        Email = c.String(maxLength: 120),
                        Password = c.String(nullable: false, maxLength: 200),
                        City = c.String(maxLength: 60),
                        Address = c.String(maxLength: 200),
                        Status = c.Int(nullable: false),
                        CreatedAtUtc = c.DateTime(nullable: false),
                        EmailOtp = c.String(maxLength: 200),
                        OtpExpiresAt = c.DateTime(),
                        IsEmailVerified = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.DonationItems",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DonorId = c.Int(nullable: false),
                        ItemName = c.String(nullable: false, maxLength: 120),
                        Quantity = c.Int(nullable: false),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Category = c.String(maxLength: 80),
                        Condition = c.String(maxLength: 60),
                        ImagePath = c.String(maxLength: 260),
                        Notes = c.String(maxLength: 500),
                        Status = c.Int(nullable: false),
                        CreatedAtUtc = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Donors", t => t.DonorId, cascadeDelete: true)
                .Index(t => t.DonorId);
            
            CreateTable(
                "dbo.Students",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FullName = c.String(nullable: false, maxLength: 80),
                        Phone = c.String(maxLength: 30),
                        Email = c.String(maxLength: 120),
                        Password = c.String(nullable: false, maxLength: 200),
                        Institute = c.String(maxLength: 120),
                        City = c.String(maxLength: 60),
                        Address = c.String(maxLength: 200),
                        Status = c.Int(nullable: false),
                        CreatedAtUtc = c.DateTime(nullable: false),
                        EmailOtp = c.String(maxLength: 200),
                        OtpExpiresAt = c.DateTime(),
                        IsEmailVerified = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.StudentApplications",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StudentId = c.Int(nullable: false),
                        ItemsNeeded = c.String(nullable: false, maxLength: 500),
                        Reason = c.String(maxLength: 800),
                        Status = c.Int(nullable: false),
                        CreatedAtUtc = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Students", t => t.StudentId, cascadeDelete: true)
                .Index(t => t.StudentId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ChatMessages", "StudentId", "dbo.Students");
            DropForeignKey("dbo.StudentApplications", "StudentId", "dbo.Students");
            DropForeignKey("dbo.ChatMessages", "DonorId", "dbo.Donors");
            DropForeignKey("dbo.DonationItems", "DonorId", "dbo.Donors");
            DropIndex("dbo.StudentApplications", new[] { "StudentId" });
            DropIndex("dbo.DonationItems", new[] { "DonorId" });
            DropIndex("dbo.ChatMessages", new[] { "StudentId" });
            DropIndex("dbo.ChatMessages", new[] { "DonorId" });
            DropTable("dbo.StudentApplications");
            DropTable("dbo.Students");
            DropTable("dbo.DonationItems");
            DropTable("dbo.Donors");
            DropTable("dbo.ChatMessages");
        }
    }
}
