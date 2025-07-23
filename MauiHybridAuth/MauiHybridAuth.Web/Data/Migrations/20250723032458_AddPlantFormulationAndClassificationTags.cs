using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MauiHybridAuth.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddPlantFormulationAndClassificationTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClassificationTags",
                table: "Compounds",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Formulations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Duration = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DoseRange = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClassificationTags = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Liposomal = table.Column<bool>(type: "bit", nullable: false),
                    Micronised = table.Column<bool>(type: "bit", nullable: false),
                    ExtendedRelease = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Formulations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Plants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Duration = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DoseRange = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClassificationTags = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FormulationConstituent",
                columns: table => new
                {
                    CompoundId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FormulationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormulationConstituent", x => new { x.CompoundId, x.FormulationId });
                    table.ForeignKey(
                        name: "FK_FormulationConstituent_Compounds_CompoundId",
                        column: x => x.CompoundId,
                        principalTable: "Compounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FormulationConstituent_Formulations_FormulationId",
                        column: x => x.FormulationId,
                        principalTable: "Formulations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlantConstituent",
                columns: table => new
                {
                    CompoundId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantConstituent", x => new { x.CompoundId, x.PlantId });
                    table.ForeignKey(
                        name: "FK_PlantConstituent_Compounds_CompoundId",
                        column: x => x.CompoundId,
                        principalTable: "Compounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlantConstituent_Plants_PlantId",
                        column: x => x.PlantId,
                        principalTable: "Plants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FormulationConstituent_FormulationId",
                table: "FormulationConstituent",
                column: "FormulationId");

            migrationBuilder.CreateIndex(
                name: "IX_PlantConstituent_PlantId",
                table: "PlantConstituent",
                column: "PlantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FormulationConstituent");

            migrationBuilder.DropTable(
                name: "PlantConstituent");

            migrationBuilder.DropTable(
                name: "Formulations");

            migrationBuilder.DropTable(
                name: "Plants");

            migrationBuilder.DropColumn(
                name: "ClassificationTags",
                table: "Compounds");
        }
    }
}
