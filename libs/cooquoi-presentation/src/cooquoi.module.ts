import { commandHandlers } from "@cooquoi/application";
import { CooquoiInfrastructureModule } from "@cooquoi/infrastructure";
import { DynamicModule, Module } from "@nestjs/common";
import { queryHandlers } from "./queries";

@Module({})
export class CooquoiModule {
	static register(): DynamicModule {
		return {
			module: CooquoiModule,
			imports: [CooquoiInfrastructureModule.register()],
			controllers: [],
			providers: [...commandHandlers, ...queryHandlers],
			exports: [CooquoiInfrastructureModule],
		};
	}
}
