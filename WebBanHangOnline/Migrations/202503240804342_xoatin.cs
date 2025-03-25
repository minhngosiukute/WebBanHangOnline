namespace WebBanHangOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class xoatin : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.tb_News", "SeoTitle");
            DropColumn("dbo.tb_News", "SeoDescription");
            DropColumn("dbo.tb_News", "SeoKeywords");
        }
        
        public override void Down()
        {
            AddColumn("dbo.tb_News", "SeoKeywords", c => c.String());
            AddColumn("dbo.tb_News", "SeoDescription", c => c.String());
            AddColumn("dbo.tb_News", "SeoTitle", c => c.String());
        }
    }
}
