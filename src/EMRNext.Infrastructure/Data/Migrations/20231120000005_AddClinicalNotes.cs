using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRNext.Infrastructure.Data.Migrations
{
    public partial class AddClinicalNotes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClinicalNotes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(nullable: false),
                    ProviderId = table.Column<int>(nullable: false),
                    EncounterId = table.Column<int>(nullable: true),
                    Type = table.Column<string>(maxLength: 100, nullable: false),
                    Title = table.Column<string>(maxLength: 200),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(maxLength: 50),
                    SignedDate = table.Column<DateTime>(nullable: true),
                    SignedBy = table.Column<string>(maxLength: 100),
                    Version = table.Column<int>(nullable: false),
                    ParentNoteId = table.Column<int>(nullable: true),
                    Tags = table.Column<string>(maxLength: 500),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 100),
                    ModifiedAt = table.Column<DateTime>(nullable: true),
                    ModifiedBy = table.Column<string>(maxLength: 100),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicalNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicalNotes_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClinicalNotes_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicalNotes_Encounters_EncounterId",
                        column: x => x.EncounterId,
                        principalTable: "Encounters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicalNotes_ClinicalNotes_ParentNoteId",
                        column: x => x.ParentNoteId,
                        principalTable: "ClinicalNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalNotes_PatientId",
                table: "ClinicalNotes",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalNotes_ProviderId",
                table: "ClinicalNotes",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalNotes_EncounterId",
                table: "ClinicalNotes",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalNotes_ParentNoteId",
                table: "ClinicalNotes",
                column: "ParentNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalNotes_CreatedAt",
                table: "ClinicalNotes",
                column: "CreatedAt");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ClinicalNotes");
        }
    }
}
