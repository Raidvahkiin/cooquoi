import { commandHandlers } from "@cooquoi/application";
import { IngredientRepository } from "@cooquoi/domain";
import {
	IngredientModel,
	IngredientSchema,
	MongoIngredientRepository,
	MongooseModule,
} from "@cooquoi/infrastructure";
import { DynamicModule, Module } from "@nestjs/common";
import { queryHandlers } from "./queries";

@Module({})
export class CooquoiModule {
	static register(): DynamicModule {
		return {
			module: CooquoiModule,
			imports: [
				MongooseModule.forFeature([
					{ name: IngredientModel.name, schema: IngredientSchema },
				]),
			],
			controllers: [],
			providers: [
				...commandHandlers,
				...queryHandlers,
				{
					provide: IngredientRepository,
					useClass: MongoIngredientRepository,
				},
			],
			exports: [IngredientRepository],
		};
	}
}
