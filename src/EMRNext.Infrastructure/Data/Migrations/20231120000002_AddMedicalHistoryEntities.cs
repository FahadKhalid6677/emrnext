using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRNext.Infrastructure.Data.Migrations
{
    public partial class AddMedicalHistoryEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Family History
            migrationBuilder.CreateTable(
                name: "FamilyHistories",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(nullable: false),
                    Relationship = table.Column<string>(maxLength: 100),
                    RelativeFirstName = table.Column<string>(maxLength: 100),
                    RelativeLastName = table.Column<string>(maxLength: 100),
                    Gender = table.Column<string>(maxLength: 50),
                    DateOfBirth = table.Column<DateTime>(nullable: true),
                    DateOfDeath = table.Column<DateTime>(nullable: true),
                    Condition = table.Column<string>(maxLength: 200),
                    ICD10Code = table.Column<string>(maxLength: 10),
                    SNOMEDCode = table.Column<string>(maxLength: 20),
                    Status = table.Column<string>(maxLength: 50),
                    AgeAtOnset = table.Column<string>(maxLength: 50),
                    IsGeneticRisk = table.Column<bool>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 100),
                    ModifiedAt = table.Column<DateTime>(nullable: true),
                    ModifiedBy = table.Column<string>(maxLength: 100),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FamilyHistories_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Social History
            migrationBuilder.CreateTable(
                name: "SocialHistories",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(nullable: false),
                    SmokingStatus = table.Column<string>(maxLength: 50),
                    PacksPerDay = table.Column<int>(nullable: true),
                    YearsSmoked = table.Column<int>(nullable: true),
                    QuitDate = table.Column<DateTime>(nullable: true),
                    AlcoholUse = table.Column<string>(maxLength: 50),
                    DrinksPerWeek = table.Column<int>(nullable: true),
                    SubstanceUse = table.Column<bool>(nullable: false),
                    SubstanceTypes = table.Column<string>(maxLength: 500),
                    ExerciseFrequency = table.Column<string>(maxLength: 50),
                    DietType = table.Column<string>(maxLength: 100),
                    Occupation = table.Column<string>(maxLength: 200),
                    EducationLevel = table.Column<string>(maxLength: 100),
                    MaritalStatus = table.Column<string>(maxLength: 50),
                    LivingArrangement = table.Column<string>(maxLength: 200),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 100),
                    ModifiedAt = table.Column<DateTime>(nullable: true),
                    ModifiedBy = table.Column<string>(maxLength: 100),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SocialHistories_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Indexes
            migrationBuilder.CreateIndex(
                name: "IX_FamilyHistories_PatientId",
                table: "FamilyHistories",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyHistories_Condition",
                table: "FamilyHistories",
                column: "Condition");

            migrationBuilder.CreateIndex(
                name: "IX_SocialHistories_PatientId",
                table: "SocialHistories",
                column: "PatientId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "SocialHistories");
            migrationBuilder.DropTable(name: "FamilyHistories");
        }
    }
}
