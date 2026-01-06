import { IngredientRepository, ProductRepository } from "@cooquoi/domain";
import { DynamicModule, Module } from "@nestjs/common";
import { TypeOrmModule } from "@nestjs/typeorm";
import { TypeOrmIngredientRepository } from "./domain-impl";
import { DbSeedable } from "./interfaces";
import { IngredientGetter, SeedDb, cooquoiTypeOrmModels } from "./typeorm";

@Module({})
export class CooquoiInfrastructureModule {
	static register(): DynamicModule {
		return {
			module: CooquoiInfrastructureModule,
			imports: [TypeOrmModule.forFeature(cooquoiTypeOrmModels)],
			providers: [
				{
					provide: DbSeedable,
					useClass: SeedDb,
				},
				{
					provide: ProductRepository,
					useClass: TypeOrmIngredientRepository,
				},
				{
					provide: IngredientRepository,
					useClass: TypeOrmIngredientRepository,
				},
				IngredientGetter,
			],
			exports: [
				DbSeedable,
				ProductRepository,
				IngredientRepository,
				IngredientGetter,
			],
		};
	}
}
