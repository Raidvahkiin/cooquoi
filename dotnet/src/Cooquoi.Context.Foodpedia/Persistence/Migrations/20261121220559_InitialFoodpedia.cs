using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cooquoi.Context.Foodpedia.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialFoodpedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "foodpedia");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .Annotation("Npgsql:PostgresExtension:unaccent", ",,");

            // Hand-written: the generated search_text column needs an IMMUTABLE unaccent.
            migrationBuilder.Sql(Projections.SearchFunctions.CreateUnaccentFunctionSql);

            migrationBuilder.CreateTable(
                name: "ingredient_search_terms",
                schema: "foodpedia",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ingredient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    locale = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    term = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false),
                    search_text = table.Column<string>(type: "text", nullable: false, computedColumnSql: "foodpedia.immutable_unaccent(lower(term))", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ingredient_search_terms", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ingredients",
                schema: "foodpedia",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    base_unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    conversions = table.Column<string>(type: "jsonb", nullable: false),
                    shelf_life_days = table.Column<int>(type: "integer", nullable: true),
                    shelf_life_storage = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    tags = table.Column<string[]>(type: "text[]", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ingredients", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                schema: "foodpedia",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tags", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ingredient_names",
                schema: "foodpedia",
                columns: table => new
                {
                    locale = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    ingredient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    primary_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    synonyms = table.Column<string[]>(type: "text[]", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    primary_name_key = table.Column<string>(type: "text", nullable: true, computedColumnSql: "lower(primary_name)", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ingredient_names", x => new { x.ingredient_id, x.locale });
                    table.ForeignKey(
                        name: "fk_ingredient_names_ingredients_ingredient_id",
                        column: x => x.ingredient_id,
                        principalSchema: "foodpedia",
                        principalTable: "ingredients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tag_labels",
                schema: "foodpedia",
                columns: table => new
                {
                    locale = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false),
                    text = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tag_labels", x => new { x.tag_id, x.locale });
                    table.ForeignKey(
                        name: "fk_tag_labels_tags_tag_id",
                        column: x => x.tag_id,
                        principalSchema: "foodpedia",
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_ingredient_names_active_primary",
                schema: "foodpedia",
                table: "ingredient_names",
                columns: new[] { "locale", "primary_name_key" },
                unique: true,
                filter: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_ingredient_search_terms_ingredient_id",
                schema: "foodpedia",
                table: "ingredient_search_terms",
                column: "ingredient_id");

            migrationBuilder.CreateIndex(
                name: "ix_ingredient_search_terms_search_text",
                schema: "foodpedia",
                table: "ingredient_search_terms",
                column: "search_text")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ux_ingredients_code",
                schema: "foodpedia",
                table: "ingredients",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_tags_code",
                schema: "foodpedia",
                table: "tags",
                column: "code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS foodpedia.immutable_unaccent(text) CASCADE;");

            migrationBuilder.DropTable(
                name: "ingredient_names",
                schema: "foodpedia");

            migrationBuilder.DropTable(
                name: "ingredient_search_terms",
                schema: "foodpedia");

            migrationBuilder.DropTable(
                name: "tag_labels",
                schema: "foodpedia");

            migrationBuilder.DropTable(
                name: "ingredients",
                schema: "foodpedia");

            migrationBuilder.DropTable(
                name: "tags",
                schema: "foodpedia");
        }
    }
}
