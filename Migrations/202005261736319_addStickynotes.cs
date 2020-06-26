namespace redHut.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addStickynotes : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserAccountModels", "stickyNotes", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserAccountModels", "stickyNotes");
        }
    }
}
