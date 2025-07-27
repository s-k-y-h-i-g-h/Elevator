using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MauiHybridAuth.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddMechanismOfActionAndEffect : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Effects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InterventionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Effects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MechanismsOfAction",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Target = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InterventionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MechanismsOfAction", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Effects_InterventionId",
                table: "Effects",
                column: "InterventionId");

            migrationBuilder.CreateIndex(
                name: "IX_MechanismsOfAction_InterventionId",
                table: "MechanismsOfAction",
                column: "InterventionId");

            // Add foreign key constraints for Effects table to all concrete intervention tables
            migrationBuilder.AddForeignKey(
                name: "FK_Effects_Compounds_InterventionId",
                table: "Effects",
                column: "InterventionId",
                principalTable: "Compounds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Effects_Plants_InterventionId",
                table: "Effects",
                column: "InterventionId",
                principalTable: "Plants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Effects_Formulations_InterventionId",
                table: "Effects",
                column: "InterventionId",
                principalTable: "Formulations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // Add foreign key constraints for MechanismsOfAction table to all concrete intervention tables
            migrationBuilder.AddForeignKey(
                name: "FK_MechanismsOfAction_Compounds_InterventionId",
                table: "MechanismsOfAction",
                column: "InterventionId",
                principalTable: "Compounds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MechanismsOfAction_Plants_InterventionId",
                table: "MechanismsOfAction",
                column: "InterventionId",
                principalTable: "Plants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MechanismsOfAction_Formulations_InterventionId",
                table: "MechanismsOfAction",
                column: "InterventionId",
                principalTable: "Formulations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop foreign key constraints for Effects
            migrationBuilder.DropForeignKey(
                name: "FK_Effects_Compounds_InterventionId",
                table: "Effects");

            migrationBuilder.DropForeignKey(
                name: "FK_Effects_Plants_InterventionId",
                table: "Effects");

            migrationBuilder.DropForeignKey(
                name: "FK_Effects_Formulations_InterventionId",
                table: "Effects");

            // Drop foreign key constraints for MechanismsOfAction
            migrationBuilder.DropForeignKey(
                name: "FK_MechanismsOfAction_Compounds_InterventionId",
                table: "MechanismsOfAction");

            migrationBuilder.DropForeignKey(
                name: "FK_MechanismsOfAction_Plants_InterventionId",
                table: "MechanismsOfAction");

            migrationBuilder.DropForeignKey(
                name: "FK_MechanismsOfAction_Formulations_InterventionId",
                table: "MechanismsOfAction");

            migrationBuilder.DropTable(
                name: "Effects");

            migrationBuilder.DropTable(
                name: "MechanismsOfAction");
        }
    }
}
