import { Column, Entity, PrimaryColumn } from "typeorm";

export interface IngredientModelProps {
	id: string;
	createdAt: Date;
	updatedAt: Date;
	name: string;
	description?: string;
}

@Entity({ name: "ingredients" })
export class IngredientModel {
	@PrimaryColumn({ type: "text" })
	public id!: string;

	@Column({ type: "timestamp" })
	public createdAt!: Date;

	@Column({ type: "timestamp" })
	public updatedAt!: Date;

	@Column({ type: "text" })
	public name!: string;

	@Column({ type: "text", nullable: true })
	public description?: string;
}
