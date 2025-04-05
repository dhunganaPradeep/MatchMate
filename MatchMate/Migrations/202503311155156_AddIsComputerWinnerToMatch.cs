namespace MatchMate.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIsComputerWinnerToMatch : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Matches", "IsComputerWinner", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Matches", "IsComputerWinner");
        }
    }
}
