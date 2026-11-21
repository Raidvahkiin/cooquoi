import { MarketModule } from '@cooquoi/market';
import { Module } from '@nestjs/common';
import { ConfigModule } from '@nestjs/config';
import {
  MediatorModule,
  NestCommandLoggerMiddleware,
} from '@utils/nestjs/cqrs';
import 'dotenv/config';
import { HealthController } from './controllers/health.controller';
import { IngredientsController } from './controllers/ingredients.controller';
import { OffersController } from './controllers/offers.controller';
import { ProductsController } from './controllers/products.controller';
import { SeedingService } from './seeding/seeding.service';

@Module({
  imports: [
    ConfigModule.forRoot({
      envFilePath: ['.env.local', '.env'],
      isGlobal: true,
    }),
    MediatorModule.forRoot({
      middlewares: [NestCommandLoggerMiddleware],
    }),
    MarketModule.register({
      database: { url: process.env.DATABASE_URL ?? '' },
    }),
  ],
  controllers: [
    HealthController,
    IngredientsController,
    OffersController,
    ProductsController,
  ],
  providers: [SeedingService],
})
export class AppModule {}
