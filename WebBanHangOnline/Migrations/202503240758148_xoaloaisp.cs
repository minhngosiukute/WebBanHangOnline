namespace WebBanHangOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class xoaloaisp : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.tb_ProductCategory", "SeoTitle");
            DropColumn("dbo.tb_ProductCategory", "SeoDescription");
            DropColumn("dbo.tb_ProductCategory", "SeoKeywords");
        }
        
        public override void Down()
        {
            AddColumn("dbo.tb_ProductCategory", "SeoKeywords", c => c.String(maxLength: 250));
            AddColumn("dbo.tb_ProductCategory", "SeoDescription", c => c.String(maxLength: 500));
            AddColumn("dbo.tb_ProductCategory", "SeoTitle", c => c.String(maxLength: 250));
        }
    }
}
