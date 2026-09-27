import { Logger } from '@nestjs/common';
import type { ILogger } from './logger';

export class NestJsLogger implements ILogger {
  log(message: string): void {
    Logger.log(message);
  }
  error(message: string, trace?: string): void {
    Logger.error(message, trace);
  }
  warn(message: string): void {
    Logger.warn(message);
  }
  debug?(message: string): void {
    Logger.debug(message);
  }
}
