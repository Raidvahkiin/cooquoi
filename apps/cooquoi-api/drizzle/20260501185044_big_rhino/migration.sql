CREATE TABLE "ingredient_components" (
	"parent_ingredient_id" uuid,
	"component_ingredient_id" uuid,
	CONSTRAINT "ingredient_components_pkey" PRIMARY KEY("parent_ingredient_id","component_ingredient_id")
);
--> statement-breakpoint
ALTER TABLE "ingredient_components" ADD CONSTRAINT "ingredient_components_parent_ingredient_id_ingredients_id_fkey" FOREIGN KEY ("parent_ingredient_id") REFERENCES "ingredients"("id") ON DELETE CASCADE;--> statement-breakpoint
ALTER TABLE "ingredient_components" ADD CONSTRAINT "ingredient_components_C2DEt5a1olON_fkey" FOREIGN KEY ("component_ingredient_id") REFERENCES "ingredients"("id") ON DELETE CASCADE;--> statement-breakpoint
ALTER TABLE "offers" ADD CONSTRAINT "offers_product_id_products_id_fkey" FOREIGN KEY ("product_id") REFERENCES "products"("id") ON DELETE CASCADE;