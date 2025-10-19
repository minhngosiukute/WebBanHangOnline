namespace WebBanHangOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class xoacata : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.tb_News", "Category_Id", "dbo.tb_Category");
            DropIndex("dbo.tb_News", new[] { "Category_Id" });
            DropColumn("dbo.tb_News", "Category_Id");
            DropTable("dbo.tb_Category");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.tb_Category",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 150),
                        Alias = c.String(),
                        Link = c.String(),
                        Description = c.String(),
                        SeoTitle = c.String(maxLength: 150),
                        SeoDescription = c.String(maxLength: 250),
                        SeoKeywords = c.String(maxLength: 150),
                        IsActive = c.Boolean(nullable: false),
                        Position = c.Int(nullable: false),
                        CreatedBy = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                        Modifiedby = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.tb_News", "Category_Id", c => c.Int());
            CreateIndex("dbo.tb_News", "Category_Id");
            AddForeignKey("dbo.tb_News", "Category_Id", "dbo.tb_Category", "Id");
        }
    }
}
