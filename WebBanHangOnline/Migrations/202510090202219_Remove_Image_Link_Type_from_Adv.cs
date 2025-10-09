namespace WebBanHangOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Remove_Image_Link_Type_from_Adv : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.tb_Adv", "Image");
            DropColumn("dbo.tb_Adv", "Link");
            DropColumn("dbo.tb_Adv", "Type");
        }
        
        public override void Down()
        {
            AddColumn("dbo.tb_Adv", "Type", c => c.Int(nullable: false));
            AddColumn("dbo.tb_Adv", "Link", c => c.String(maxLength: 500));
            AddColumn("dbo.tb_Adv", "Image", c => c.String(maxLength: 500));
        }
    }
}
