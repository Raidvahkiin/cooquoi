import { Global, Module } from "@nestjs/common";
import { DatetimeProvider, SystemDatetimeProvider } from "@libs/core";

@Global()
@Module({
	providers: [
		{
			provide: DatetimeProvider,
			useClass: SystemDatetimeProvider,
		},
	],
	exports: [DatetimeProvider],
})
export class SystemDatetimeModule {}
