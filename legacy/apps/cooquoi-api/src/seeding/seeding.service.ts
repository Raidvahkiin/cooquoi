import { MarketSeederService, type SeedData } from '@cooquoi/market';
import { Injectable, Logger, OnApplicationBootstrap } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import seedData from './seed-data.json';

@Injectable()
export class SeedingService implements OnApplicationBootstrap {
  private readonly logger = new Logger(SeedingService.name);

  constructor(
    private readonly config: ConfigService,
    private readonly marketSeeder: MarketSeederService,
  ) {}

  async onApplicationBootstrap(): Promise<void> {
    const isDev = this.config.get<string>('NODE_ENV') !== 'production';
    const enabled =
      this.config.get<string>('DATABASE_SEEDING_ENABLED') === 'true';

    if (!isDev || !enabled) return;

    this.logger.log('DATABASE_SEEDING_ENABLED=true (dev) — starting seed...');
    await this.marketSeeder.seed(seedData as SeedData);
  }
}
