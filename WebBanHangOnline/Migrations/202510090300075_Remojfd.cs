namespace WebBanHangOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Remojfd : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tb_Adv", "Link", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.tb_Adv", "Link");
        }
    }
}
