using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cooquoi.Platform.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialPlatform : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "platform");

            migrationBuilder.CreateTable(
                name: "idempotent_requests",
                schema: "platform",
                columns: table => new
                {
                    subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    key = table.Column<Guid>(type: "uuid", nullable: false),
                    request_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    state = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    claimed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status_code = table.Column<int>(type: "integer", nullable: true),
                    content_type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    location = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    e_tag = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    body = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_idempotent_requests", x => new { x.subject, x.key });
                });

            migrationBuilder.CreateIndex(
                name: "ix_idempotent_requests_created_at",
                schema: "platform",
                table: "idempotent_requests",
                column: "created_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "idempotent_requests",
                schema: "platform");
        }
    }
}
