namespace WebBanHangOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class metmet : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.tb_SupportTicketMessage",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TicketId = c.Int(nullable: false),
                        Message = c.String(nullable: false),
                        SenderId = c.String(maxLength: 256),
                        SenderName = c.String(maxLength: 150),
                        IsFromStaff = c.Boolean(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.tb_SupportTicket", t => t.TicketId, cascadeDelete: true)
                .Index(t => t.TicketId);
            
            CreateTable(
                "dbo.tb_SupportTicket",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TicketCode = c.String(nullable: false, maxLength: 16),
                        FullName = c.String(nullable: false, maxLength: 150),
                        Email = c.String(nullable: false, maxLength: 200),
                        PhoneNumber = c.String(maxLength: 20),
                        Subject = c.String(nullable: false, maxLength: 200),
                        Status = c.String(maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false),
                        UpdatedDate = c.DateTime(),
                        AssignedToUserId = c.String(maxLength: 256),
                        AssignedToName = c.String(maxLength: 150),
                        LastRepliedAt = c.DateTime(),
                        LastRepliedBy = c.String(maxLength: 256),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.tb_SupportTicketMessage", "TicketId", "dbo.tb_SupportTicket");
            DropIndex("dbo.tb_SupportTicketMessage", new[] { "TicketId" });
            DropTable("dbo.tb_SupportTicket");
            DropTable("dbo.tb_SupportTicketMessage");
        }
    }
}
