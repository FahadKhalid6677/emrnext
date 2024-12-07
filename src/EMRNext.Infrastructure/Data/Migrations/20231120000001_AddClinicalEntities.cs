using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRNext.Infrastructure.Data.Migrations
{
    public partial class AddClinicalEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Allergies
            migrationBuilder.CreateTable(
                name: "Allergies",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(nullable: false),
                    Type = table.Column<string>(maxLength: 100),
                    Allergen = table.Column<string>(maxLength: 200),
                    Reaction = table.Column<string>(maxLength: 500),
                    Severity = table.Column<string>(maxLength: 50),
                    Status = table.Column<string>(maxLength: 50),
                    OnsetDate = table.Column<DateTime>(nullable: true),
                    EndDate = table.Column<DateTime>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 100),
                    ModifiedAt = table.Column<DateTime>(nullable: true),
                    ModifiedBy = table.Column<string>(maxLength: 100),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Allergies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Allergies_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Problems
            migrationBuilder.CreateTable(
                name: "Problems",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(nullable: false),
                    Description = table.Column<string>(maxLength: 500),
                    ICD10Code = table.Column<string>(maxLength: 10),
                    SNOMEDCode = table.Column<string>(maxLength: 20),
                    Status = table.Column<string>(maxLength: 50),
                    Severity = table.Column<string>(maxLength: 50),
                    OnsetDate = table.Column<DateTime>(nullable: true),
                    EndDate = table.Column<DateTime>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 100),
                    ModifiedAt = table.Column<DateTime>(nullable: true),
                    ModifiedBy = table.Column<string>(maxLength: 100),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Problems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Problems_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Immunizations
            migrationBuilder.CreateTable(
                name: "Immunizations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(nullable: false),
                    VaccineName = table.Column<string>(maxLength: 200),
                    CVXCode = table.Column<string>(maxLength: 10),
                    AdministeredDate = table.Column<DateTime>(nullable: false),
                    Route = table.Column<string>(maxLength: 50),
                    Site = table.Column<string>(maxLength: 50),
                    Dose = table.Column<string>(maxLength: 50),
                    LotNumber = table.Column<string>(maxLength: 50),
                    Manufacturer = table.Column<string>(maxLength: 100),
                    ExpirationDate = table.Column<DateTime>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 100),
                    ModifiedAt = table.Column<DateTime>(nullable: true),
                    ModifiedBy = table.Column<string>(maxLength: 100),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Immunizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Immunizations_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Indexes
            migrationBuilder.CreateIndex(
                name: "IX_Allergies_PatientId",
                table: "Allergies",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Problems_PatientId",
                table: "Problems",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Problems_ICD10Code",
                table: "Problems",
                column: "ICD10Code");

            migrationBuilder.CreateIndex(
                name: "IX_Immunizations_PatientId",
                table: "Immunizations",
                column: "PatientId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Immunizations");
            migrationBuilder.DropTable(name: "Problems");
            migrationBuilder.DropTable(name: "Allergies");
        }
    }
}
