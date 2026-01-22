using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sports.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class New : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlayerRatings",
                columns: table => new
                {
                    PlayerRatingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameId = table.Column<int>(type: "int", nullable: false),
                    RaterId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RatedUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SkillRating = table.Column<int>(type: "int", nullable: false),
                    WasOnTime = table.Column<bool>(type: "bit", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerRatings", x => x.PlayerRatingId);
                    table.ForeignKey(
                        name: "FK_PlayerRatings_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "GameId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "EmployeeDirectory",
                keyColumn: "EmployeeId",
                keyValue: 1,
                columns: new[] { "Email", "FullName" },
                values: new object[] { "vijay.rakesh@techcorp.com", "Vijay Rakesh" });

            migrationBuilder.UpdateData(
                table: "EmployeeDirectory",
                keyColumn: "EmployeeId",
                keyValue: 2,
                columns: new[] { "Email", "FullName" },
                values: new object[] { "virat.kohli@techcorp.com", "Virat Kohli" });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerRatings_GameId",
                table: "PlayerRatings",
                column: "GameId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerRatings");

            migrationBuilder.UpdateData(
                table: "EmployeeDirectory",
                keyColumn: "EmployeeId",
                keyValue: 1,
                columns: new[] { "Email", "FullName" },
                values: new object[] { "john.doe@techcorp.com", "John Doe" });

            migrationBuilder.UpdateData(
                table: "EmployeeDirectory",
                keyColumn: "EmployeeId",
                keyValue: 2,
                columns: new[] { "Email", "FullName" },
                values: new object[] { "jane.smith@techcorp.com", "Jane Smith" });
        }
    }
}
