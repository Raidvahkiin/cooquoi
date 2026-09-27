import { Inject, Optional } from '@nestjs/common';
import { Command } from '@nestjs/cqrs';
import { CommandMiddleware, EnableLoggingDecorator } from '../decorators';

import {
  CommandMiddlewareContext,
  CommandMiddlewareNext,
  ICommandMiddlewareHandler,
} from './command-handler.middleware';
import type { ILogger } from '../logging/logger';
import { MIDDLEWARE_LOGGER } from '../logging/logger';
import { NestJsLogger } from '../logging/nestjs-logger';

function maskSensitiveData(obj: unknown, sensitiveKeys: string[]): void {
  if (typeof obj === 'object' && obj !== null) {
    const record = obj as Record<string, unknown>;
    for (const key in record) {
      if (Object.hasOwn(record, key)) {
        if (
          sensitiveKeys.map((k) => k.toLowerCase()).includes(key.toLowerCase())
        ) {
          record[key] = '***';
        } else if (typeof record[key] === 'object' && record[key] !== null) {
          maskSensitiveData(record[key], sensitiveKeys);
        }
      }
    }
  }
}

@CommandMiddleware(Command)
export class NestCommandLoggerMiddleware
  implements ICommandMiddlewareHandler<Command<void>, void>
{
  private readonly loggers: ILogger[] = [new NestJsLogger()];

  constructor(
    @Optional() @Inject(MIDDLEWARE_LOGGER) loggers?: ILogger[] | undefined,
  ) {
    if (loggers && loggers.length > 0) {
      this.loggers = loggers;
    }
  }

  async process(
    ctx: CommandMiddlewareContext<Command<void>>,
    next: CommandMiddlewareNext<Command<void>, void>,
  ): Promise<void> {
    const command = ctx.command;

    if (EnableLoggingDecorator.existIn(command)) {
      const clonedCommand = structuredClone(command);
      maskSensitiveData(
        clonedCommand,
        EnableLoggingDecorator.getConfigFrom(command).ignore ?? [],
      );
      const commandDetail = JSON.stringify(clonedCommand);
      this.debug(
        `[${ctx.id} - ${command.constructor.name}] Executing with params: ${commandDetail}}`,
      );
    } else {
      this.debug(`[${ctx.id} - ${command.constructor.name}] Executing.`);
    }

    const now = new Date();
    try {
      return await next(ctx);
    } catch (error) {
      const err = error instanceof Error ? error : new Error(`${error}`);
      this.error(
        `[${ctx.id} - ${command.constructor.name}] Error: ${err.message}. Stack: ${err.stack}`,
      );
      throw error;
    } finally {
      const end = new Date();
      const diff = end.getTime() - now.getTime();
      this.debug(
        `[${ctx.id} - ${command.constructor.name}] Finish execution. Took ${diff}ms`,
      );
    }
  }

  private debug(message: string): void {
    for (const logger of this.loggers) {
      logger.debug?.(message);
    }
  }

  private error(message: string): void {
    for (const logger of this.loggers) {
      logger.error?.(message);
    }
  }
}
