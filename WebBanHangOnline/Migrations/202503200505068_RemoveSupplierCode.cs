namespace WebBanHangOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveSupplierCode : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.tb_Supplier", "SupplierCode");
        }
        
        public override void Down()
        {
            AddColumn("dbo.tb_Supplier", "SupplierCode", c => c.String(nullable: false, maxLength: 50));
        }
    }
}
