namespace MatchMate.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateDatabaseSchema : DbMigration
    {
        public override void Up()
        {
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Matches", "WinnerId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Matches", "Player2Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.Matches", "Player1Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.Matches", "GameId", "dbo.Games");
            DropIndex("dbo.Matches", new[] { "WinnerId" });
            DropIndex("dbo.Matches", new[] { "Player2Id" });
            DropIndex("dbo.Matches", new[] { "Player1Id" });
            DropIndex("dbo.Matches", new[] { "GameId" });
            DropTable("dbo.Matches");
            DropTable("dbo.Games");
        }
    }
}
