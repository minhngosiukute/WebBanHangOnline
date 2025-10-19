namespace WebBanHangOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class xoasp : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.tb_Product", "ProductCode");
            DropColumn("dbo.tb_Product", "Detail");
        }
        
        public override void Down()
        {
            AddColumn("dbo.tb_Product", "Detail", c => c.String());
            AddColumn("dbo.tb_Product", "ProductCode", c => c.String(maxLength: 50));
        }
    }
}
