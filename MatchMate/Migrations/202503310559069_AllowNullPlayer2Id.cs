using System.Data.Entity.Migrations;

public partial class AllowNullPlayer2Id : DbMigration
{
    public override void Up()
    {
        AlterColumn("dbo.Matches", "Player2Id", c => c.String(maxLength: 128, nullable: true));
    }

    public override void Down()
    {
        AlterColumn("dbo.Matches", "Player2Id", c => c.String(maxLength: 128, nullable: false));
    }
}