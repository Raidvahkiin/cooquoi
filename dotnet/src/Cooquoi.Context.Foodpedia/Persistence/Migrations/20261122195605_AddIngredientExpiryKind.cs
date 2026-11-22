using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cooquoi.Context.Foodpedia.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIngredientExpiryKind : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "expiry_kind",
                schema: "foodpedia",
                table: "ingredients",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Estimated");

            // Existing rows get Estimated; the default is dropped so every new ingredient must state its kind.
            migrationBuilder.Sql("ALTER TABLE foodpedia.ingredients ALTER COLUMN expiry_kind DROP DEFAULT;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "expiry_kind",
                schema: "foodpedia",
                table: "ingredients");
        }
    }
}
