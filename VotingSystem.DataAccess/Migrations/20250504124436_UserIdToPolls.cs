using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VotingSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UserIdToPolls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Polls",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Polls_CreatedByUserId",
                table: "Polls",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Polls_AspNetUsers_CreatedByUserId",
                table: "Polls",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Polls_AspNetUsers_CreatedByUserId",
                table: "Polls");

            migrationBuilder.DropIndex(
                name: "IX_Polls_CreatedByUserId",
                table: "Polls");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Polls");
        }
    }
}
