namespace WebBanHangOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class xoatinss : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.tb_News", "CategoryId", "dbo.tb_Category");
            DropIndex("dbo.tb_News", new[] { "CategoryId" });
            RenameColumn(table: "dbo.tb_News", name: "CategoryId", newName: "Category_Id");
            AlterColumn("dbo.tb_News", "Category_Id", c => c.Int());
            CreateIndex("dbo.tb_News", "Category_Id");
            AddForeignKey("dbo.tb_News", "Category_Id", "dbo.tb_Category", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.tb_News", "Category_Id", "dbo.tb_Category");
            DropIndex("dbo.tb_News", new[] { "Category_Id" });
            AlterColumn("dbo.tb_News", "Category_Id", c => c.Int(nullable: false));
            RenameColumn(table: "dbo.tb_News", name: "Category_Id", newName: "CategoryId");
            CreateIndex("dbo.tb_News", "CategoryId");
            AddForeignKey("dbo.tb_News", "CategoryId", "dbo.tb_Category", "Id", cascadeDelete: true);
        }
    }
}
