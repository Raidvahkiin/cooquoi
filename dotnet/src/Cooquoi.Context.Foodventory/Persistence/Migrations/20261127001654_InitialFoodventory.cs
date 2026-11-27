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
                name: "ingredient_stocks",
                schema: "foodventory",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    household_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ingredient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    base_unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    expiry_kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ingredient_stocks", x => x.id);
                });

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

            migrationBuilder.CreateTable(
                name: "stock_items",
                schema: "foodventory",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pantry_id = table.Column<Guid>(type: "uuid", nullable: true),
                    entered_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    entered_unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    remaining_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    normalized_amount = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    expires_on = table.Column<DateOnly>(type: "date", nullable: true),
                    expiry_source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    added_on = table.Column<DateOnly>(type: "date", nullable: false),
                    source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    household_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ingredient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ingredient_stock_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stock_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_stock_items_ingredient_stocks_ingredient_stock_id",
                        column: x => x.ingredient_stock_id,
                        principalSchema: "foodventory",
                        principalTable: "ingredient_stocks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_ingredient_stocks_household_ingredient",
                schema: "foodventory",
                table: "ingredient_stocks",
                columns: new[] { "household_id", "ingredient_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_pantries_household_name_key",
                schema: "foodventory",
                table: "pantries",
                columns: new[] { "household_id", "name_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_stock_items_household_expires_on",
                schema: "foodventory",
                table: "stock_items",
                columns: new[] { "household_id", "expires_on" });

            migrationBuilder.CreateIndex(
                name: "ix_stock_items_household_pantry",
                schema: "foodventory",
                table: "stock_items",
                columns: new[] { "household_id", "pantry_id" });

            migrationBuilder.CreateIndex(
                name: "ix_stock_items_ingredient_stock_id",
                schema: "foodventory",
                table: "stock_items",
                column: "ingredient_stock_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pantries",
                schema: "foodventory");

            migrationBuilder.DropTable(
                name: "stock_items",
                schema: "foodventory");

            migrationBuilder.DropTable(
                name: "ingredient_stocks",
                schema: "foodventory");

            migrationBuilder.Sql($"DROP FUNCTION IF EXISTS {FoodventoryDbContext.Schema}.{PantryNameKey.UnaccentFunction}(text);");
        }
    }
}
