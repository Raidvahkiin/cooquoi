import { defineRelations } from 'drizzle-orm';
import { pgTable, uuid } from 'drizzle-orm/pg-core';
import { ingredientComponents, ingredients } from './ingredient.entity';
import { offers } from './offers.entity';
import { products } from './product.entity';

export const productIngredients = pgTable('product_ingredients', {
  productId: uuid('product_id')
    .notNull()
    .references(() => products.id, { onDelete: 'cascade' }),
  ingredientId: uuid('ingredient_id')
    .notNull()
    .references(() => ingredients.id, { onDelete: 'cascade' }),
});

export type ProductIngredient = typeof productIngredients.$inferSelect;

export const relations = defineRelations(
  {
    products,
    ingredients,
    ingredientComponents,
    offers,
    productIngredients,
  },
  (r) => ({
    products: {
      ingredients: r.many.ingredients({
        from: r.products.id.through(r.productIngredients.productId),
        to: r.ingredients.id.through(r.productIngredients.ingredientId),
      }),
      offers: r.many.offers({
        from: r.products.id,
        to: r.offers.productId,
      }),
    },
    ingredients: {
      components: r.many.ingredients({
        from: r.ingredients.id.through(
          r.ingredientComponents.parentIngredientId,
        ),
        to: r.ingredients.id.through(
          r.ingredientComponents.componentIngredientId,
        ),
        alias: 'ingredient_components',
      }),
      composedIn: r.many.ingredients({
        from: r.ingredients.id.through(
          r.ingredientComponents.componentIngredientId,
        ),
        to: r.ingredients.id.through(
          r.ingredientComponents.parentIngredientId,
        ),
        alias: 'ingredient_composed_in',
      }),
    },
    offers: {
      product: r.one.products({
        from: r.offers.productId,
        to: r.products.id,
      }),
    },
  }),
);

export const schema = {
  products,
  ingredients,
  ingredientComponents,
  offers,
  productIngredients,
  relations,
};
