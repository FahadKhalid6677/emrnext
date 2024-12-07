using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRNext.Infrastructure.Data.Migrations
{
    public partial class AddCarePlansAndAppointments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Care Plans
            migrationBuilder.CreateTable(
                name: "CarePlans",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(nullable: false),
                    ProviderId = table.Column<int>(nullable: false),
                    EncounterId = table.Column<int>(nullable: true),
                    Title = table.Column<string>(maxLength: 200, nullable: false),
                    Description = table.Column<string>(maxLength: 1000),
                    Category = table.Column<string>(maxLength: 100),
                    Status = table.Column<string>(maxLength: 50),
                    StartDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: true),
                    PrimaryDiagnosis = table.Column<string>(maxLength: 200),
                    DiagnosisCode = table.Column<string>(maxLength: 20),
                    Goals = table.Column<string>(maxLength: 1000),
                    Outcomes = table.Column<string>(maxLength: 1000),
                    NextReviewDate = table.Column<DateTime>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 100),
                    ModifiedAt = table.Column<DateTime>(nullable: true),
                    ModifiedBy = table.Column<string>(maxLength: 100),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarePlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarePlans_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CarePlans_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CarePlans_Encounters_EncounterId",
                        column: x => x.EncounterId,
                        principalTable: "Encounters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Care Plan Activities
            migrationBuilder.CreateTable(
                name: "CarePlanActivities",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CarePlanId = table.Column<int>(nullable: false),
                    Title = table.Column<string>(maxLength: 200, nullable: false),
                    Description = table.Column<string>(maxLength: 1000),
                    Category = table.Column<string>(maxLength: 100),
                    Status = table.Column<string>(maxLength: 50),
                    Priority = table.Column<string>(maxLength: 50),
                    StartDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: true),
                    CompletionStatus = table.Column<string>(maxLength: 50),
                    CompletionDate = table.Column<DateTime>(nullable: true),
                    CompletedBy = table.Column<string>(maxLength: 100),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 100),
                    ModifiedAt = table.Column<DateTime>(nullable: true),
                    ModifiedBy = table.Column<string>(maxLength: 100),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarePlanActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarePlanActivities_CarePlans_CarePlanId",
                        column: x => x.CarePlanId,
                        principalTable: "CarePlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Appointments
            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(nullable: false),
                    ProviderId = table.Column<int>(nullable: false),
                    Type = table.Column<string>(maxLength: 100),
                    Status = table.Column<string>(maxLength: 50),
                    StartTime = table.Column<DateTime>(nullable: false),
                    EndTime = table.Column<DateTime>(nullable: false),
                    Duration = table.Column<int>(nullable: false),
                    Location = table.Column<string>(maxLength: 200),
                    Room = table.Column<string>(maxLength: 50),
                    AppointmentReason = table.Column<string>(maxLength: 500),
                    ChiefComplaint = table.Column<string>(maxLength: 500),
                    Instructions = table.Column<string>(maxLength: 1000),
                    IsCancelled = table.Column<bool>(nullable: false),
                    CancellationReason = table.Column<string>(maxLength: 500),
                    CancellationDate = table.Column<DateTime>(nullable: true),
                    ResultingEncounterId = table.Column<int>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 100),
                    ModifiedAt = table.Column<DateTime>(nullable: true),
                    ModifiedBy = table.Column<string>(maxLength: 100),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Appointments_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Appointments_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Appointments_Encounters_ResultingEncounterId",
                        column: x => x.ResultingEncounterId,
                        principalTable: "Encounters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Indexes
            migrationBuilder.CreateIndex(
                name: "IX_CarePlans_PatientId",
                table: "CarePlans",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_CarePlans_ProviderId",
                table: "CarePlans",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_CarePlans_EncounterId",
                table: "CarePlans",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_CarePlanActivities_CarePlanId",
                table: "CarePlanActivities",
                column: "CarePlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PatientId",
                table: "Appointments",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ProviderId",
                table: "Appointments",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ResultingEncounterId",
                table: "Appointments",
                column: "ResultingEncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_StartTime",
                table: "Appointments",
                column: "StartTime");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "CarePlanActivities");
            migrationBuilder.DropTable(name: "CarePlans");
            migrationBuilder.DropTable(name: "Appointments");
        }
    }
}
