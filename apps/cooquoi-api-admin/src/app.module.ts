import { Module } from "@nestjs/common";
import { ConfigModule, ConfigService } from "@nestjs/config";
import { CqrsModule } from "@nestjs/cqrs";
import { MongooseModule } from "@cooquoi/infrastructure";
import { CooquoiModule } from "@cooquoi/presentation";
import { SystemDatetimeModule } from "@libs/nest-extension";
import { IngredientsController } from "./controllers";

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
		SystemDatetimeModule,
		CooquoiModule.register(),
	],
	controllers: [IngredientsController],
})
export class AppModule {}
