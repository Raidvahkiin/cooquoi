import { MarketModule } from '@cooquoi/market';
import { Module } from '@nestjs/common';
import { ConfigModule } from '@nestjs/config';
import { CqrsModule } from '@nestjs/cqrs';
import 'dotenv/config';
import { HealthController } from './app.controller';

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
  controllers: [HealthController],
})
export class AppModule {}
