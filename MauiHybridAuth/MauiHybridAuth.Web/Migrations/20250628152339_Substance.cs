using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MauiHybridAuth.Web.Migrations
{
    /// <inheritdoc />
    public partial class Substance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClassificationTags",
                table: "Compound",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DoseRange",
                table: "Compound",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DurationInMinutes",
                table: "Compound",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClassificationTags",
                table: "Compound");

            migrationBuilder.DropColumn(
                name: "DoseRange",
                table: "Compound");

            migrationBuilder.DropColumn(
                name: "DurationInMinutes",
                table: "Compound");
        }
    }
}
