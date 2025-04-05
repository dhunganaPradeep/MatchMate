namespace MatchMate.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixOldPasswordField : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "OldPassword", c => c.String(maxLength: 500));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "OldPassword");
        }
    }
}
