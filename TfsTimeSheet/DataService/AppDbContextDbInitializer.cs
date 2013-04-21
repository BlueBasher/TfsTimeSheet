namespace TfsTimeSheet.DataService
{
    using System.Data.Entity;
    using TfsTimeSheet.Migrations;

    internal class AppDbContextDbInitializer : MigrateDatabaseToLatestVersion<AppDbContext, MigrationConfiguration>
    {
    }
}
