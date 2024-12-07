using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRNext.Core.Infrastructure.Migrations
{
    public partial class AppointmentSchedulingEnhancements : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // AppointmentTypes table
            migrationBuilder.CreateTable(
                name: "AppointmentTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    Description = table.Column<string>(maxLength: 500, nullable: true),
                    Duration = table.Column<int>(nullable: false),
                    Color = table.Column<string>(maxLength: 7, nullable: true),
                    RequiresPreAuth = table.Column<bool>(nullable: false),
                    AllowsTelehealth = table.Column<bool>(nullable: false),
                    DefaultInstructions = table.Column<string>(nullable: true),
                    SpecialtyId = table.Column<int>(nullable: true),
                    ProviderId = table.Column<int>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppointmentTypes_Specialties_SpecialtyId",
                        column: x => x.SpecialtyId,
                        principalTable: "Specialties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppointmentTypes_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // ProviderSchedules table
            migrationBuilder.CreateTable(
                name: "ProviderSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderId = table.Column<int>(nullable: false),
                    LocationId = table.Column<int>(nullable: false),
                    DayOfWeek = table.Column<int>(nullable: false),
                    StartTime = table.Column<TimeSpan>(nullable: false),
                    EndTime = table.Column<TimeSpan>(nullable: false),
                    IsAvailable = table.Column<bool>(nullable: false),
                    EffectiveDate = table.Column<DateTime>(nullable: false),
                    ExpirationDate = table.Column<DateTime>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProviderSchedules_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProviderSchedules_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // ProviderTimeOff table
            migrationBuilder.CreateTable(
                name: "ProviderTimeOff",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderId = table.Column<int>(nullable: false),
                    StartDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: false),
                    Reason = table.Column<string>(maxLength: 500, nullable: true),
                    IsApproved = table.Column<bool>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderTimeOff", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProviderTimeOff_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // AppointmentWaitList table
            migrationBuilder.CreateTable(
                name: "AppointmentWaitList",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(nullable: false),
                    AppointmentTypeId = table.Column<int>(nullable: false),
                    PreferredProviderId = table.Column<int>(nullable: true),
                    EarliestDate = table.Column<DateTime>(nullable: false),
                    LatestDate = table.Column<DateTime>(nullable: true),
                    Priority = table.Column<int>(nullable: false),
                    Status = table.Column<string>(maxLength: 50, nullable: false),
                    Notes = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentWaitList", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppointmentWaitList_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppointmentWaitList_AppointmentTypes_AppointmentTypeId",
                        column: x => x.AppointmentTypeId,
                        principalTable: "AppointmentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppointmentWaitList_Providers_PreferredProviderId",
                        column: x => x.PreferredProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // GroupAppointments table
            migrationBuilder.CreateTable(
                name: "GroupAppointments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    Description = table.Column<string>(maxLength: 500, nullable: true),
                    ProviderId = table.Column<int>(nullable: false),
                    LocationId = table.Column<int>(nullable: false),
                    AppointmentTypeId = table.Column<int>(nullable: false),
                    StartTime = table.Column<DateTime>(nullable: false),
                    EndTime = table.Column<DateTime>(nullable: false),
                    MaxParticipants = table.Column<int>(nullable: false),
                    MinParticipants = table.Column<int>(nullable: false),
                    Status = table.Column<string>(maxLength: 50, nullable: false),
                    Instructions = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupAppointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupAppointments_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupAppointments_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupAppointments_AppointmentTypes_AppointmentTypeId",
                        column: x => x.AppointmentTypeId,
                        principalTable: "AppointmentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // GroupAppointmentParticipants table
            migrationBuilder.CreateTable(
                name: "GroupAppointmentParticipants",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupAppointmentId = table.Column<int>(nullable: false),
                    PatientId = table.Column<int>(nullable: false),
                    Status = table.Column<string>(maxLength: 50, nullable: false),
                    JoinedDate = table.Column<DateTime>(nullable: false),
                    Notes = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupAppointmentParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupAppointmentParticipants_GroupAppointments_GroupAppointmentId",
                        column: x => x.GroupAppointmentId,
                        principalTable: "GroupAppointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupAppointmentParticipants_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_AppointmentTypes_Name",
                table: "AppointmentTypes",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentTypes_ProviderId",
                table: "AppointmentTypes",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentTypes_SpecialtyId",
                table: "AppointmentTypes",
                column: "SpecialtyId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderSchedules_ProviderId_LocationId_DayOfWeek",
                table: "ProviderSchedules",
                columns: new[] { "ProviderId", "LocationId", "DayOfWeek" });

            migrationBuilder.CreateIndex(
                name: "IX_ProviderTimeOff_ProviderId_StartDate_EndDate",
                table: "ProviderTimeOff",
                columns: new[] { "ProviderId", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentWaitList_PatientId",
                table: "AppointmentWaitList",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentWaitList_AppointmentTypeId",
                table: "AppointmentWaitList",
                column: "AppointmentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupAppointments_ProviderId_StartTime",
                table: "GroupAppointments",
                columns: new[] { "ProviderId", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_GroupAppointmentParticipants_GroupAppointmentId_PatientId",
                table: "GroupAppointmentParticipants",
                columns: new[] { "GroupAppointmentId", "PatientId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "GroupAppointmentParticipants");
            migrationBuilder.DropTable(name: "GroupAppointments");
            migrationBuilder.DropTable(name: "AppointmentWaitList");
            migrationBuilder.DropTable(name: "ProviderTimeOff");
            migrationBuilder.DropTable(name: "ProviderSchedules");
            migrationBuilder.DropTable(name: "AppointmentTypes");
        }
    }
}
