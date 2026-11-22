using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cooquoi.Context.Household.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CamelCaseEnumCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ADR 0021: enum codes are camelCase; convert the PascalCase values stored before (Down restores them).
            migrationBuilder.Sql("UPDATE household.memberships SET role = lower(left(role, 1)) || substring(role from 2) WHERE role IS NOT NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE household.memberships SET role = upper(left(role, 1)) || substring(role from 2) WHERE role IS NOT NULL;");
        }
    }
}
