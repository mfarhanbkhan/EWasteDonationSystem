using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace EWasteDonationSystem.Models
{
    /// <summary>
    /// Entity Framework 6 Code-First context.
    /// Database is created automatically on first run.
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext() : base("DefaultConnection")
        {
        }

        public DbSet<Donor> Donors { get; set; }
        public DbSet<DonationItem> DonationItems { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<StudentApplication> StudentApplications { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DonationItem>()
                .HasRequired(x => x.Donor)
                .WithMany(x => x.DonationItems)
                .HasForeignKey(x => x.DonorId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<StudentApplication>()
                .HasRequired(x => x.Student)
                .WithMany(x => x.Applications)
                .HasForeignKey(x => x.StudentId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<ChatMessage>()
                .HasOptional(x => x.Donor)
                .WithMany(x => x.ChatMessages)
                .HasForeignKey(x => x.DonorId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<ChatMessage>()
                .HasOptional(x => x.Student)
                .WithMany(x => x.ChatMessages)
                .HasForeignKey(x => x.StudentId)
                .WillCascadeOnDelete(true);
        }
    }
}
