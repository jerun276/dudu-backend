using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using CupidLearn.Infrastructure.Data;

#nullable disable

namespace CupidLearn.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260527100000_AddChildIdToLessonProgress")]
    public partial class AddChildIdToLessonProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ChildId",
                table: "LessonProgress",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LessonProgress_ChildId_LessonId",
                table: "LessonProgress",
                columns: new[] { "ChildId", "LessonId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LessonProgress_ChildId_LessonId",
                table: "LessonProgress");

            migrationBuilder.DropColumn(
                name: "ChildId",
                table: "LessonProgress");
        }
    }
}
