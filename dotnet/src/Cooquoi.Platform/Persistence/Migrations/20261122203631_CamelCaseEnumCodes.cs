using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cooquoi.Platform.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CamelCaseEnumCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ADR 0021: enum codes are camelCase; convert the PascalCase values stored before (Down restores them).
            migrationBuilder.Sql("UPDATE platform.idempotent_requests SET state = lower(left(state, 1)) || substring(state from 2) WHERE state IS NOT NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE platform.idempotent_requests SET state = upper(left(state, 1)) || substring(state from 2) WHERE state IS NOT NULL;");
        }
    }
}
