import { Ingredient, IngredientRepository } from "@cooquoi/domain";
import { Injectable } from "@nestjs/common";
import { InjectRepository } from "@nestjs/typeorm";
import { Repository } from "typeorm";
import { IngredientMapper } from "../typeorm/mappers";
import { IngredientModel } from "../typeorm/models";

@Injectable()
export class TypeOrmIngredientRepository implements IngredientRepository {
	constructor(
		@InjectRepository(IngredientModel)
		private readonly ingredientRepository: Repository<IngredientModel>,
	) {}

	async save(ingredient: Ingredient): Promise<void> {
		const model = IngredientMapper.fromEntity(ingredient).toModel();
		await this.ingredientRepository.save(model);
	}

	async deleteById(id: string): Promise<void> {
		await this.ingredientRepository.delete({ id });
	}

	async findById(id: string): Promise<Ingredient | null> {
		const model = await this.ingredientRepository.findOne({ where: { id } });
		if (!model) {
			return null;
		}
		return IngredientMapper.fromModel(model).toEntity();
	}
}
