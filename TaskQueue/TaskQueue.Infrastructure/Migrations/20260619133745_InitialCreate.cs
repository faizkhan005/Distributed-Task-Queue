using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskQueue.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "job_failures",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_record_id = table.Column<Guid>(type: "uuid", nullable: false),
                    hangfire_job_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    job_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    payload_json = table.Column<string>(type: "jsonb", nullable: false),
                    error_message = table.Column<string>(type: "text", nullable: false),
                    stack_trace = table.Column<string>(type: "text", nullable: true),
                    total_attempts = table.Column<int>(type: "integer", nullable: false),
                    failed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    requeued = table.Column<bool>(type: "boolean", nullable: false),
                    requeued_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_failures", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "job_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    hangfire_job_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    queue = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "default"),
                    payload_json = table.Column<string>(type: "jsonb", nullable: false),
                    attempt_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    max_attempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 4),
                    last_error_message = table.Column<string>(type: "text", nullable: true),
                    last_error_stack_trace = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dead_lettered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    correlation_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_records", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_job_failures_failed_at",
                table: "job_failures",
                column: "failed_at");

            migrationBuilder.CreateIndex(
                name: "ix_job_failures_job_record_id",
                table: "job_failures",
                column: "job_record_id");

            migrationBuilder.CreateIndex(
                name: "ix_job_failures_requeued",
                table: "job_failures",
                column: "requeued");

            migrationBuilder.CreateIndex(
                name: "ix_job_records_correlation_id",
                table: "job_records",
                column: "correlation_id");

            migrationBuilder.CreateIndex(
                name: "ix_job_records_created_at",
                table: "job_records",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_job_records_hangfire_job_id",
                table: "job_records",
                column: "hangfire_job_id");

            migrationBuilder.CreateIndex(
                name: "ix_job_records_status",
                table: "job_records",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_job_records_type",
                table: "job_records",
                column: "type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "job_failures");

            migrationBuilder.DropTable(
                name: "job_records");
        }
    }
}
