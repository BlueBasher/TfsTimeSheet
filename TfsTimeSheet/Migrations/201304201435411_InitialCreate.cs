namespace TfsTimeSheet.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TimeSheetItems",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        WorkItemId = c.Int(nullable: false),
                        Project = c.String(nullable: false, maxLength: 250),
                        ServerUrl = c.String(nullable: false, maxLength: 1000),
                        Name = c.String(nullable: false, maxLength: 1000),
                        FirstDayOfWeek = c.DateTime(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 250),
                        Monday = c.DateTime(nullable: false),
                        Tuesday = c.DateTime(nullable: false),
                        Wednesday = c.DateTime(nullable: false),
                        Thursday = c.DateTime(nullable: false),
                        Friday = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.TimeSheetItems");
        }
    }
}
