using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthSystem.Migrations
{
    /// <inheritdoc />
    public partial class addedQuestionToFAQ2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "FAQs",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.UpdateData(
                table: "FAQs",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Answer", "Question" },
                values: new object[] { "You are welcome.", "Thank you" });

            migrationBuilder.UpdateData(
                table: "FAQs",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Answer", "Question" },
                values: new object[] { "To reset your password, go to profile section.", "How do I reset my password?" });

            migrationBuilder.UpdateData(
                table: "FAQs",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Answer", "Question" },
                values: new object[] { "You can contact support by clicking on their socials.", "How can I contact support?" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "FAQs",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Answer", "Question" },
                values: new object[] { ".", "What is my recent message sent?" });

            migrationBuilder.UpdateData(
                table: "FAQs",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Answer", "Question" },
                values: new object[] { "You are welcome.", "Thank you" });

            migrationBuilder.UpdateData(
                table: "FAQs",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Answer", "Question" },
                values: new object[] { "To reset your password, go to profile section.", "How do I reset my password?" });

            migrationBuilder.InsertData(
                table: "FAQs",
                columns: new[] { "Id", "Answer", "Question" },
                values: new object[] { 5, "You can contact support by clicking on their socials.", "How can I contact support?" });
        }
    }
}
