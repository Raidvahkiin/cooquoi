import { Module } from "@nestjs/common";
import { ConfigModule, ConfigService } from "@nestjs/config";
import { CqrsModule } from "@nestjs/cqrs";
import {
	IngredientModel,
	IngredientSchema,
	MongooseModule,
} from "@cooquoi/infrastructure";
import { CooquoiModule } from "@cooquoi/presentation";
import { SystemDatetimeModule } from "@libs/utils-nestjs";
import { IngredientsController } from "./controllers";
import { SeedDb } from "@cooquoi/infrastructure";
import { OnApplicationBootstrapLifecycle } from "./lifecycle/on-application-bootstrap";

@Module({
	imports: [
		ConfigModule.forRoot({
			isGlobal: true,
			envFilePath: [".env.local", ".env"],
		}),
		CqrsModule.forRoot(),
		CqrsModule,
		MongooseModule.forRootAsync({
			imports: [ConfigModule],
			inject: [ConfigService],
			useFactory: (configService: ConfigService) => ({
				uri: configService.get<string>("DB_MONGO_URI"),
			}),
		}),
		MongooseModule.forFeature([
			{
				name: IngredientModel.name,
				schema: IngredientSchema,
			},
		]),
		SystemDatetimeModule,
		CooquoiModule.register(),
	],
	controllers: [IngredientsController],
	providers: [OnApplicationBootstrapLifecycle, SeedDb],
})
export class AppModule {}
