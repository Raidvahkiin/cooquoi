import { Module } from "@nestjs/common";
import { MongooseModule } from "@cooquoi/infrastructure";
import { CooquoiModule } from "@cooquoi/presentation";
import { SystemDatetimeModule } from "@libs/nest-extension";

@Module({
	imports: [
		MongooseModule.forRoot("mongodb://localhost:27017/cooquoi-admin"),
		SystemDatetimeModule,
		CooquoiModule.register(),
	],
})
export class AppModule {}
