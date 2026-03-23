using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CupidLearn.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddModuleOrderIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderIndex",
                table: "Modules",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderIndex",
                table: "Modules");
        }
    }
}
