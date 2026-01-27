using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sports.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NewR : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RelatedGameId",
                table: "Notifications",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RelatedGameId",
                table: "Notifications");
        }
    }
}
