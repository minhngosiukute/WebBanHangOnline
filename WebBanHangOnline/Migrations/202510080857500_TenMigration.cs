namespace WebBanHangOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TenMigration : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.tb_Subscribe", "Email", c => c.String(nullable: false, maxLength: 256));
            CreateIndex("dbo.tb_Subscribe", "Email", unique: true, name: "IX_tb_Subscribe_Email");
        }
        
        public override void Down()
        {
            DropIndex("dbo.tb_Subscribe", "IX_tb_Subscribe_Email");
            AlterColumn("dbo.tb_Subscribe", "Email", c => c.String(nullable: false));
        }
    }
}
