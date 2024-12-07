using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRNext.Infrastructure.Data.Migrations
{
    public partial class AddMedicationsAndLabs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Medications
            migrationBuilder.CreateTable(
                name: "Medications",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(nullable: false),
                    ProviderId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 200, nullable: false),
                    GenericName = table.Column<string>(maxLength: 200),
                    NDCCode = table.Column<string>(maxLength: 20),
                    RxNorm = table.Column<string>(maxLength: 20),
                    Strength = table.Column<string>(maxLength: 50),
                    Form = table.Column<string>(maxLength: 50),
                    Route = table.Column<string>(maxLength: 50),
                    Dosage = table.Column<string>(maxLength: 100),
                    Frequency = table.Column<string>(maxLength: 100),
                    StartDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: true),
                    Status = table.Column<string>(maxLength: 50),
                    IsHighRisk = table.Column<bool>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 100),
                    ModifiedAt = table.Column<DateTime>(nullable: true),
                    ModifiedBy = table.Column<string>(maxLength: 100),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Medications_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Medications_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Lab Orders
            migrationBuilder.CreateTable(
                name: "LabOrders",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(nullable: false),
                    ProviderId = table.Column<int>(nullable: false),
                    EncounterId = table.Column<int>(nullable: true),
                    OrderNumber = table.Column<string>(maxLength: 50),
                    OrderType = table.Column<string>(maxLength: 100),
                    Status = table.Column<string>(maxLength: 50),
                    Priority = table.Column<string>(maxLength: 50),
                    OrderDate = table.Column<DateTime>(nullable: false),
                    CollectionDate = table.Column<DateTime>(nullable: true),
                    TestCode = table.Column<string>(maxLength: 20),
                    TestName = table.Column<string>(maxLength: 200),
                    LOINC = table.Column<string>(maxLength: 20),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 100),
                    ModifiedAt = table.Column<DateTime>(nullable: true),
                    ModifiedBy = table.Column<string>(maxLength: 100),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabOrders_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LabOrders_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabOrders_Encounters_EncounterId",
                        column: x => x.EncounterId,
                        principalTable: "Encounters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Lab Results
            migrationBuilder.CreateTable(
                name: "LabResults",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LabOrderId = table.Column<int>(nullable: false),
                    PatientId = table.Column<int>(nullable: false),
                    TestCode = table.Column<string>(maxLength: 20),
                    TestName = table.Column<string>(maxLength: 200),
                    LOINC = table.Column<string>(maxLength: 20),
                    Value = table.Column<string>(maxLength: 100),
                    Units = table.Column<string>(maxLength: 50),
                    ReferenceRange = table.Column<string>(maxLength: 100),
                    Interpretation = table.Column<string>(maxLength: 50),
                    IsAbnormal = table.Column<bool>(nullable: false),
                    IsCritical = table.Column<bool>(nullable: false),
                    ResultDate = table.Column<DateTime>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 100),
                    ModifiedAt = table.Column<DateTime>(nullable: true),
                    ModifiedBy = table.Column<string>(maxLength: 100),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabResults_LabOrders_LabOrderId",
                        column: x => x.LabOrderId,
                        principalTable: "LabOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LabResults_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Indexes
            migrationBuilder.CreateIndex(
                name: "IX_Medications_PatientId",
                table: "Medications",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Medications_ProviderId",
                table: "Medications",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_Medications_NDCCode",
                table: "Medications",
                column: "NDCCode");

            migrationBuilder.CreateIndex(
                name: "IX_LabOrders_PatientId",
                table: "LabOrders",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_LabOrders_ProviderId",
                table: "LabOrders",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_LabOrders_EncounterId",
                table: "LabOrders",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_LabResults_LabOrderId",
                table: "LabResults",
                column: "LabOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_LabResults_PatientId",
                table: "LabResults",
                column: "PatientId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "LabResults");
            migrationBuilder.DropTable(name: "LabOrders");
            migrationBuilder.DropTable(name: "Medications");
        }
    }
}
