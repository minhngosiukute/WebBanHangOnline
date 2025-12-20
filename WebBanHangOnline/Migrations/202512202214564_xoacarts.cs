namespace WebBanHangOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class xoacarts : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.tb_Cart", "CreatedBy");
            DropColumn("dbo.tb_Cart", "CreatedDate");
            DropColumn("dbo.tb_Cart", "ModifiedDate");
            DropColumn("dbo.tb_Cart", "Modifiedby");
        }
        
        public override void Down()
        {
            AddColumn("dbo.tb_Cart", "Modifiedby", c => c.String());
            AddColumn("dbo.tb_Cart", "ModifiedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.tb_Cart", "CreatedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.tb_Cart", "CreatedBy", c => c.String());
        }
    }
}
