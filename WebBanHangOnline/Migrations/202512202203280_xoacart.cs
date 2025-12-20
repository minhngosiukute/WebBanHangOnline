namespace WebBanHangOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class xoacart : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.tb_Cart", "UserId", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.tb_CartItem", "ProductId");
            CreateIndex("dbo.tb_Cart", "UserId");
            AddForeignKey("dbo.tb_Cart", "UserId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
            AddForeignKey("dbo.tb_CartItem", "ProductId", "dbo.tb_Product", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.tb_CartItem", "ProductId", "dbo.tb_Product");
            DropForeignKey("dbo.tb_Cart", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.tb_Cart", new[] { "UserId" });
            DropIndex("dbo.tb_CartItem", new[] { "ProductId" });
            AlterColumn("dbo.tb_Cart", "UserId", c => c.String(nullable: false));
        }
    }
}
