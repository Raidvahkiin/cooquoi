using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cooquoi.Context.Foodventory.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialFoodventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "foodventory");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:unaccent", ",,");

            // Hand-written: the generated name_key column needs an IMMUTABLE unaccent.
            migrationBuilder.Sql(PantryNameKey.CreateUnaccentFunctionSql);

            migrationBuilder.CreateTable(
                name: "pantries",
                schema: "foodventory",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    household_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    storage = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    name_key = table.Column<string>(type: "text", nullable: true, computedColumnSql: "foodventory.immutable_unaccent(lower(name))", stored: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pantries", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_pantries_household_name_key",
                schema: "foodventory",
                table: "pantries",
                columns: new[] { "household_id", "name_key" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pantries",
                schema: "foodventory");

            migrationBuilder.Sql($"DROP FUNCTION IF EXISTS {FoodventoryDbContext.Schema}.{PantryNameKey.UnaccentFunction}(text);");
        }
    }
}
