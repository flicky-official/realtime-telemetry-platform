using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApp.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialTelemetry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "devices",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_devices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "telemetry_samples",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RecordedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReceivedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CpuTemperatureC = table.Column<double>(type: "double precision", nullable: true),
                    CpuUsagePercent = table.Column<double>(type: "double precision", nullable: true),
                    CpuClockMhz = table.Column<double>(type: "double precision", nullable: true),
                    MemoryUsedBytes = table.Column<long>(type: "bigint", nullable: true),
                    MemoryTotalBytes = table.Column<long>(type: "bigint", nullable: true),
                    IntegratedGpuTemperatureC = table.Column<double>(type: "double precision", nullable: true),
                    IntegratedGpuClockMhz = table.Column<double>(type: "double precision", nullable: true),
                    NvidiaGpuTemperatureC = table.Column<double>(type: "double precision", nullable: true),
                    NvidiaGpuUsagePercent = table.Column<double>(type: "double precision", nullable: true),
                    NvidiaGpuClockMhz = table.Column<double>(type: "double precision", nullable: true),
                    NvidiaMemoryUsedBytes = table.Column<long>(type: "bigint", nullable: true),
                    NvidiaMemoryTotalBytes = table.Column<long>(type: "bigint", nullable: true),
                    CpuFanRpm = table.Column<int>(type: "integer", nullable: true),
                    GpuFanRpm = table.Column<int>(type: "integer", nullable: true),
                    SsdTemperatureC = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_telemetry_samples", x => x.Id);
                    table.ForeignKey(
                        name: "FK_telemetry_samples_devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_telemetry_samples_DeviceId_RecordedAtUtc_Id",
                table: "telemetry_samples",
                columns: new[] { "DeviceId", "RecordedAtUtc", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "telemetry_samples");

            migrationBuilder.DropTable(
                name: "devices");
        }
    }
}
