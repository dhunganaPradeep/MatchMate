namespace MatchMate.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLastLoginTime : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "LastLoginTime", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "LastLoginTime");
        }
    }
}
