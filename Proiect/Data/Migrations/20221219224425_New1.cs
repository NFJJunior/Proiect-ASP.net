using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Proiect.Data.Migrations
{
    /// <inheritdoc />
    public partial class New1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserGroupModerators_UserId",
                table: "UserGroupModerators");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserGroupModerators_UserId",
                table: "UserGroupModerators",
                column: "UserId",
                unique: true);
        }
    }
}
