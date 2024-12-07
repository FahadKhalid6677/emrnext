using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRNext.Infrastructure.Data.Migrations
{
    public partial class AddConsentsAndDirectives : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Consents
            migrationBuilder.CreateTable(
                name: "Consents",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(nullable: false),
                    Type = table.Column<string>(maxLength: 100, nullable: false),
                    Description = table.Column<string>(maxLength: 1000),
                    Status = table.Column<string>(maxLength: 50),
                    EffectiveDate = table.Column<DateTime>(nullable: false),
                    ExpirationDate = table.Column<DateTime>(nullable: true),
                    DocumentPath = table.Column<string>(maxLength: 500),
                    SignedBy = table.Column<string>(maxLength: 100),
                    SignedDate = table.Column<DateTime>(nullable: true),
                    WitnessName = table.Column<string>(maxLength: 200),
                    WitnessSignatureDate = table.Column<DateTime>(nullable: true),
                    Notes = table.Column<string>(maxLength: 1000),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 100),
                    ModifiedAt = table.Column<DateTime>(nullable: true),
                    ModifiedBy = table.Column<string>(maxLength: 100),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Consents_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Advance Directives
            migrationBuilder.CreateTable(
                name: "AdvanceDirectives",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(nullable: false),
                    Type = table.Column<string>(maxLength: 100, nullable: false),
                    Description = table.Column<string>(maxLength: 1000),
                    Status = table.Column<string>(maxLength: 50),
                    EffectiveDate = table.Column<DateTime>(nullable: false),
                    ReviewDate = table.Column<DateTime>(nullable: true),
                    DocumentPath = table.Column<string>(maxLength: 500),
                    SignedBy = table.Column<string>(maxLength: 100),
                    SignedDate = table.Column<DateTime>(nullable: true),
                    WitnessName = table.Column<string>(maxLength: 200),
                    WitnessSignatureDate = table.Column<DateTime>(nullable: true),
                    CustodianName = table.Column<string>(maxLength: 200),
                    CustodianContact = table.Column<string>(maxLength: 200),
                    Notes = table.Column<string>(maxLength: 1000),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 100),
                    ModifiedAt = table.Column<DateTime>(nullable: true),
                    ModifiedBy = table.Column<string>(maxLength: 100),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvanceDirectives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdvanceDirectives_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Indexes
            migrationBuilder.CreateIndex(
                name: "IX_Consents_PatientId",
                table: "Consents",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Consents_Type",
                table: "Consents",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Consents_EffectiveDate",
                table: "Consents",
                column: "EffectiveDate");

            migrationBuilder.CreateIndex(
                name: "IX_AdvanceDirectives_PatientId",
                table: "AdvanceDirectives",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvanceDirectives_Type",
                table: "AdvanceDirectives",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_AdvanceDirectives_EffectiveDate",
                table: "AdvanceDirectives",
                column: "EffectiveDate");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Consents");
            migrationBuilder.DropTable(name: "AdvanceDirectives");
        }
    }
}
