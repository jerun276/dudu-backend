using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CupidLearn.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProgressAttemptSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Score",
                table: "Attempts",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "AttemptType",
                table: "Attempts",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "ExerciseId",
                table: "Attempts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdempotencyKey",
                table: "Attempts",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPassed",
                table: "Attempts",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LessonId",
                table: "Attempts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "OccurredAt",
                table: "Attempts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Success",
                table: "Attempts",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Attempts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.CreateIndex(
                name: "IX_Attempts_UserId_IdempotencyKey",
                table: "Attempts",
                columns: new[] { "UserId", "IdempotencyKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Attempts_UserId_IdempotencyKey",
                table: "Attempts");

            migrationBuilder.DropColumn(
                name: "AttemptType",
                table: "Attempts");

            migrationBuilder.DropColumn(
                name: "ExerciseId",
                table: "Attempts");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                table: "Attempts");

            migrationBuilder.DropColumn(
                name: "IsPassed",
                table: "Attempts");

            migrationBuilder.DropColumn(
                name: "LessonId",
                table: "Attempts");

            migrationBuilder.DropColumn(
                name: "OccurredAt",
                table: "Attempts");

            migrationBuilder.DropColumn(
                name: "Success",
                table: "Attempts");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Attempts");

            migrationBuilder.AlterColumn<int>(
                name: "Score",
                table: "Attempts",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
