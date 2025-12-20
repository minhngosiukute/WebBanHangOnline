namespace WebBanHangOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class cart : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.tb_CartItem",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CartId = c.Int(nullable: false),
                        ProductId = c.Int(nullable: false),
                        ProductName = c.String(nullable: false),
                        Alias = c.String(),
                        CategoryName = c.String(),
                        ProductImg = c.String(),
                        Quantity = c.Int(nullable: false),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TotalPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.tb_Cart", t => t.CartId, cascadeDelete: true)
                .Index(t => t.CartId);
            
            CreateTable(
                "dbo.tb_Cart",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false),
                        CreatedBy = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                        Modifiedby = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.tb_CartItem", "CartId", "dbo.tb_Cart");
            DropIndex("dbo.tb_CartItem", new[] { "CartId" });
            DropTable("dbo.tb_Cart");
            DropTable("dbo.tb_CartItem");
        }
    }
}
