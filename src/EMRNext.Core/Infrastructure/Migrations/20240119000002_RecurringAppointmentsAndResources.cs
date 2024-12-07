using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRNext.Core.Infrastructure.Migrations
{
    public partial class RecurringAppointmentsAndResources : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // RecurringAppointments table
            migrationBuilder.CreateTable(
                name: "RecurringAppointments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(nullable: false),
                    ProviderId = table.Column<int>(nullable: false),
                    AppointmentTypeId = table.Column<int>(nullable: false),
                    StartDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: true),
                    RecurrencePattern = table.Column<string>(maxLength: 50, nullable: false),
                    RecurrenceInterval = table.Column<int>(nullable: false),
                    DaysOfWeek = table.Column<string>(maxLength: 50, nullable: true),
                    TimeOfDay = table.Column<TimeSpan>(nullable: false),
                    LocationId = table.Column<int>(nullable: false),
                    Status = table.Column<string>(maxLength: 50, nullable: false),
                    Notes = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecurringAppointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecurringAppointments_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecurringAppointments_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecurringAppointments_AppointmentTypes_AppointmentTypeId",
                        column: x => x.AppointmentTypeId,
                        principalTable: "AppointmentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // RecurringAppointmentExceptions table
            migrationBuilder.CreateTable(
                name: "RecurringAppointmentExceptions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecurringAppointmentId = table.Column<int>(nullable: false),
                    ExceptionDate = table.Column<DateTime>(nullable: false),
                    Reason = table.Column<string>(maxLength: 500, nullable: true),
                    NewDateTime = table.Column<DateTime>(nullable: true),
                    Status = table.Column<string>(maxLength: 50, nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecurringAppointmentExceptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecurringAppointmentExceptions_RecurringAppointments_RecurringAppointmentId",
                        column: x => x.RecurringAppointmentId,
                        principalTable: "RecurringAppointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Resources table
            migrationBuilder.CreateTable(
                name: "Resources",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    Type = table.Column<string>(maxLength: 50, nullable: false),
                    LocationId = table.Column<int>(nullable: false),
                    Description = table.Column<string>(maxLength: 500, nullable: true),
                    Capacity = table.Column<int>(nullable: true),
                    IsShared = table.Column<bool>(nullable: false),
                    RequiresApproval = table.Column<bool>(nullable: false),
                    Status = table.Column<string>(maxLength: 50, nullable: false),
                    MaintenanceSchedule = table.Column<string>(nullable: true),
                    LastMaintenanceDate = table.Column<DateTime>(nullable: true),
                    NextMaintenanceDate = table.Column<DateTime>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Resources_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // ResourceSchedules table
            migrationBuilder.CreateTable(
                name: "ResourceSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResourceId = table.Column<int>(nullable: false),
                    AppointmentId = table.Column<int>(nullable: true),
                    StartTime = table.Column<DateTime>(nullable: false),
                    EndTime = table.Column<DateTime>(nullable: false),
                    Purpose = table.Column<string>(maxLength: 200, nullable: true),
                    Status = table.Column<string>(maxLength: 50, nullable: false),
                    Notes = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResourceSchedules_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResourceSchedules_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_RecurringAppointments_PatientId",
                table: "RecurringAppointments",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringAppointments_ProviderId_StartDate",
                table: "RecurringAppointments",
                columns: new[] { "ProviderId", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_RecurringAppointmentExceptions_RecurringAppointmentId_ExceptionDate",
                table: "RecurringAppointmentExceptions",
                columns: new[] { "RecurringAppointmentId", "ExceptionDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Resources_LocationId_Type",
                table: "Resources",
                columns: new[] { "LocationId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_ResourceSchedules_ResourceId_StartTime_EndTime",
                table: "ResourceSchedules",
                columns: new[] { "ResourceId", "StartTime", "EndTime" });

            migrationBuilder.CreateIndex(
                name: "IX_ResourceSchedules_AppointmentId",
                table: "ResourceSchedules",
                column: "AppointmentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ResourceSchedules");
            migrationBuilder.DropTable(name: "Resources");
            migrationBuilder.DropTable(name: "RecurringAppointmentExceptions");
            migrationBuilder.DropTable(name: "RecurringAppointments");
        }
    }
}
