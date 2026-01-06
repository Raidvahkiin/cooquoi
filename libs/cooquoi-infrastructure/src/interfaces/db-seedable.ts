export abstract class DbSeedable {
	abstract seedInitialData(): Promise<void>;
}
