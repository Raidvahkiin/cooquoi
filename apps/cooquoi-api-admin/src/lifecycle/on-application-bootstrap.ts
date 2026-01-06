import { DbSeedable } from "@cooquoi/presentation";
import { Injectable, OnApplicationBootstrap } from "@nestjs/common";

@Injectable()
export class OnApplicationBootstrapLifecycle implements OnApplicationBootstrap {
	constructor(private readonly _seedDb: DbSeedable) {}

	async onApplicationBootstrap() {
		await this._seedDb.seedInitialData();
	}
}
