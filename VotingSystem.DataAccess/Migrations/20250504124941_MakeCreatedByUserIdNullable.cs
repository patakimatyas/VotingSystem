using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VotingSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class MakeCreatedByUserIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Polls_AspNetUsers_CreatedByUserId",
                table: "Polls");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByUserId",
                table: "Polls",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_Polls_AspNetUsers_CreatedByUserId",
                table: "Polls",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Polls_AspNetUsers_CreatedByUserId",
                table: "Polls");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByUserId",
                table: "Polls",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Polls_AspNetUsers_CreatedByUserId",
                table: "Polls",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
