import { MarketModule } from '@cooquoi/market';
import { Module } from '@nestjs/common';
import { ConfigModule } from '@nestjs/config';
import { CqrsModule } from '@nestjs/cqrs';
import 'dotenv/config';
import { HealthController } from './controllers/health.controller';
import { IngredientController } from './controllers/ingredient.controller';

@Module({
  imports: [
    ConfigModule.forRoot({
      envFilePath: ['.env.local', '.env'],
      isGlobal: true,
    }),
    CqrsModule.forRoot(),
    MarketModule.register({
      database: { url: process.env.DATABASE_URL ?? '' },
    }),
  ],
  controllers: [HealthController, IngredientController],
})
export class AppModule {}
