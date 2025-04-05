namespace MatchMate.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addingMatches : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Matches", "MatchGuid", c => c.String(maxLength: 100));
            AddColumn("dbo.Matches", "WaitingForOpponent", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Matches", "WaitingForOpponent");
            DropColumn("dbo.Matches", "MatchGuid");
        }
    }
}
