import { Column, Entity, JoinTable, ManyToMany, PrimaryColumn } from "typeorm";
import { IngredientModel } from "./ingredient.model";

export interface ProductIngredientQuantity {
	ingredientId: string;
	quantityAmount: number;
	quantityUnitType: string;
	quantityUnitValue: string;
}

export interface ProductModelProps {
	id: string;
	createdAt: Date;
	updatedAt: Date;
	name: string;
	customAttributes: Record<string, string | number | boolean>;
	ingredientQuantities: ProductIngredientQuantity[];
}

@Entity("products")
export class ProductModel {
	@PrimaryColumn({ type: "text" })
	public id!: string;

	@Column({ type: "timestamp" })
	public createdAt!: Date;

	@Column({ type: "timestamp" })
	public updatedAt!: Date;

	@Column({ type: "text" })
	public name!: string;

	@Column({ type: "json" })
	public aliases!: string[];

	@Column({ type: "json" })
	public ingredientQuantities!: ProductIngredientQuantity[];

	@Column({ type: "json" })
	public customAttributes!: Record<string, string | number | boolean>;

	// #region reference fields

	@ManyToMany(() => IngredientModel)
	@JoinTable({
		name: "product_ingredients",
		joinColumn: { name: "productId", referencedColumnName: "id" },
		inverseJoinColumn: { name: "ingredientId", referencedColumnName: "id" },
	})
	public ingredients!: IngredientModel[];

	// #endregion
}
