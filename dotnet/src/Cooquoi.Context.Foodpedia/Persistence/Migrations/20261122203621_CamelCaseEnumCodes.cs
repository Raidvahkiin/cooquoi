using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cooquoi.Context.Foodpedia.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CamelCaseEnumCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ADR 0021: enum codes are camelCase; convert the PascalCase values stored before (Down restores them).
            migrationBuilder.Sql("UPDATE foodpedia.ingredients SET expiry_kind = lower(left(expiry_kind, 1)) || substring(expiry_kind from 2) WHERE expiry_kind IS NOT NULL;");
            migrationBuilder.Sql("UPDATE foodpedia.ingredients SET status = lower(left(status, 1)) || substring(status from 2) WHERE status IS NOT NULL;");
            migrationBuilder.Sql("UPDATE foodpedia.ingredients SET shelf_life_storage = lower(left(shelf_life_storage, 1)) || substring(shelf_life_storage from 2) WHERE shelf_life_storage IS NOT NULL;");
            migrationBuilder.Sql("UPDATE foodpedia.tags SET status = lower(left(status, 1)) || substring(status from 2) WHERE status IS NOT NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE foodpedia.ingredients SET expiry_kind = upper(left(expiry_kind, 1)) || substring(expiry_kind from 2) WHERE expiry_kind IS NOT NULL;");
            migrationBuilder.Sql("UPDATE foodpedia.ingredients SET status = upper(left(status, 1)) || substring(status from 2) WHERE status IS NOT NULL;");
            migrationBuilder.Sql("UPDATE foodpedia.ingredients SET shelf_life_storage = upper(left(shelf_life_storage, 1)) || substring(shelf_life_storage from 2) WHERE shelf_life_storage IS NOT NULL;");
            migrationBuilder.Sql("UPDATE foodpedia.tags SET status = upper(left(status, 1)) || substring(status from 2) WHERE status IS NOT NULL;");
        }
    }
}
