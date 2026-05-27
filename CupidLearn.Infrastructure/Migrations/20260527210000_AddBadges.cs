using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CupidLearn.Infrastructure.Migrations
{
    public partial class AddBadges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BadgeDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Icon = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BadgeDefinitions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BadgeDefinitions_Key",
                table: "BadgeDefinitions",
                column: "Key",
                unique: true);

            migrationBuilder.CreateTable(
                name: "EarnedBadges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChildId = table.Column<Guid>(type: "uuid", nullable: false),
                    BadgeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EarnedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EarnedBadges", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EarnedBadges_UserId_ChildId_BadgeId",
                table: "EarnedBadges",
                columns: new[] { "UserId", "ChildId", "BadgeId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "EarnedBadges");
            migrationBuilder.DropTable(name: "BadgeDefinitions");
        }
    }
}
