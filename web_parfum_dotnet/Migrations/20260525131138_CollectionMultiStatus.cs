using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebParfum.Migrations
{
    /// <inheritdoc />
    public partial class CollectionMultiStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Collections_UserId",
                table: "Collections");

            migrationBuilder.CreateIndex(
                name: "IX_Collections_UserId_PerfumeId_Status",
                table: "Collections",
                columns: new[] { "UserId", "PerfumeId", "Status" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Collections_UserId_PerfumeId_Status",
                table: "Collections");

            migrationBuilder.CreateIndex(
                name: "IX_Collections_UserId",
                table: "Collections",
                column: "UserId");
        }
    }
}
