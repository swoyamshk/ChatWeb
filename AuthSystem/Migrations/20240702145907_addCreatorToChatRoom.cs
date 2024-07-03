using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthSystem.Migrations
{
    /// <inheritdoc />
    public partial class addCreatorToChatRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatorId",
                table: "ChatRooms",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            // Set a default valid CreatorId value for existing rows
            migrationBuilder.Sql("UPDATE ChatRooms SET CreatorId = (SELECT TOP 1 Id FROM AspNetUsers)");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRooms_CreatorId",
                table: "ChatRooms",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRooms_AspNetUsers_CreatorId",
                table: "ChatRooms",
                column: "CreatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatRooms_AspNetUsers_CreatorId",
                table: "ChatRooms");

            migrationBuilder.DropIndex(
                name: "IX_ChatRooms_CreatorId",
                table: "ChatRooms");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "ChatRooms");
        }
    }
}
