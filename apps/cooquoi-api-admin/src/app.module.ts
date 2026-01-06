import { DynamicModule, Module } from "@nestjs/common";
import { ConfigModule } from "@nestjs/config";
import { CqrsModule } from "@nestjs/cqrs";
import { CooquoiModule, cooquoiTypeOrmModels } from "@cooquoi/presentation";
import { SystemDatetimeModule } from "@libs/utils-nestjs";
import {
	IngredientsController,
	IngredientsGridController,
} from "./controllers";
import { OnApplicationBootstrapLifecycle } from "./lifecycle/on-application-bootstrap";
import { TypeOrmModule } from "@nestjs/typeorm";
import { appConfigSchema } from "./config";

@Module({
	imports: [
		ConfigModule.forRoot({
			isGlobal: true,
			envFilePath: [".env.local", ".env"],
		}),
		CqrsModule.forRoot(),
		SystemDatetimeModule,
	],
	controllers: [IngredientsController, IngredientsGridController],
	providers: [OnApplicationBootstrapLifecycle],
})
export class AppModule {
	static register(): DynamicModule {
		return {
			module: AppModule,
			imports: [
				TypeOrmModule.forRootAsync({
					useFactory: () => {
						const config = appConfigSchema.parse({});
						console.log("config", JSON.stringify(config, null, 2));
						return {
							type: "postgres",
							entities: cooquoiTypeOrmModels,
							...config.database.postgres,
						};
					},
				}),
				CooquoiModule.register(),
			],
			providers: [],
			exports: [],
		};
	}
}
