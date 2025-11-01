namespace WebBanHangOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class xoaproducca : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.tb_ProductCategory", "Description");
            DropColumn("dbo.tb_ProductCategory", "Icon");
        }
        
        public override void Down()
        {
            AddColumn("dbo.tb_ProductCategory", "Icon", c => c.String(maxLength: 250));
            AddColumn("dbo.tb_ProductCategory", "Description", c => c.String());
        }
    }
}
