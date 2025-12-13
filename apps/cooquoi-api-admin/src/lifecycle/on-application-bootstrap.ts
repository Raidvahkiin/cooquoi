import { SeedDb } from "@cooquoi/infrastructure";
import { Injectable, OnApplicationBootstrap } from "@nestjs/common";

@Injectable()
export class OnApplicationBootstrapLifecycle implements OnApplicationBootstrap {
	constructor(private readonly _seedDb: SeedDb) {}

	async onApplicationBootstrap() {
		await this._seedDb.seedInitialData();
	}
}
