namespace EWasteDonationSystem.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<EWasteDonationSystem.Models.AppDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(EWasteDonationSystem.Models.AppDbContext context)
        {
            context.Users.AddOrUpdate(
                x => x.Email,
                new EWasteDonationSystem.Models.User
                {
                    Email = "admin@gmail.com",
                    FullName = "Administrator",
                    Password = "Admin@123",
                    RoleType = "Admin",
                    IsActive = true,
                    IsEmailVerified = true,
                    CreatedAtUtc = DateTime.UtcNow
                });
        }
    }
}
