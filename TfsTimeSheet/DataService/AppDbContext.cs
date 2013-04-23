namespace TfsTimeSheet.DataService
{
    using System.Data.Entity;
    using TfsTimeSheet.Models;

    public class AppDbContext : DbContext
    {
        public DbSet<TimeSheetItem> TimeSheetItems { get; set; }

        public AppDbContext()
        {
            Database.SetInitializer(new AppDbContextDbInitializer());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<TimeSheetItem>().Ignore(t => t.IsNotifying);
            modelBuilder.Entity<TimeSheetItem>().Ignore(t => t.Url);
			modelBuilder.Entity<TimeSheetItem>().Ignore(t => t.WorkRemaining);
			modelBuilder.Entity<TimeSheetItem>().Ignore(t => t.IsClosed);
            modelBuilder.Entity<TimeSheetItem>().Ignore(t => t.IsTotal);
        }
    }
}
