using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthSystem.Migrations
{
    public partial class addPrivateChatMessage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove the table creation if the table already exists
            // migrationBuilder.CreateTable(
            //    name: "ChatMessages",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        SenderId = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        ChatRoomId = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_ChatMessages", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_ChatMessages_AspNetUsers_SenderId",
            //            column: x => x.SenderId,
            //            principalTable: "AspNetUsers",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_ChatMessages_ChatRooms_ChatRoomId",
            //            column: x => x.ChatRoomId,
            //            principalTable: "ChatRooms",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            // Ensure indexes are created if they do not already exist
            /*f (!IndexExists(migrationBuilder, "ChatMessages", "IX_ChatMessages_ChatRoomId"))
             {
                 migrationBuilder.CreateIndex(
                     name: "IX_ChatMessages_ChatRoomId",
                     table: "ChatMessages",
                     column: "ChatRoomId");
             }

             if (!IndexExists(migrationBuilder, "ChatMessages", "IX_ChatMessages_SenderId"))
             {
                 migrationBuilder.CreateIndex(
                     name: "IX_ChatMessages_SenderId",
                     table: "ChatMessages",
                     column: "SenderId");
             }
            */
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove the DropTable call if the table was not created by this migration
            // migrationBuilder.DropTable(
            //    name: "ChatMessages");
        }


    }
}