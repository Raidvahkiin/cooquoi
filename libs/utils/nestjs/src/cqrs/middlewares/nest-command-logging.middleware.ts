import { Logger } from '@nestjs/common';
import { Command } from '@nestjs/cqrs';
import { CommandMiddleware, EnableLoggingDecorator } from '../decorators';

import {
  CommandMiddlewareContext,
  CommandMiddlewareNext,
  ICommandMiddlewareHandler,
} from './command-handler.middleware';

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
      Logger.debug(
        `[${ctx.id} - ${command.constructor.name}] Executing with params: ${commandDetail}}`,
      );
    } else {
      Logger.debug(`[${ctx.id} - ${command.constructor.name}] Executing.`);
    }

    const now = new Date();
    try {
      return await next(ctx);
    } catch (error) {
      const err = error instanceof Error ? error : new Error(`${error}`);
      Logger.error(
        `[${ctx.id} - ${command.constructor.name}] Error: ${err.message}. Stack: ${err.stack}`,
      );
      throw error;
    } finally {
      const end = new Date();
      const diff = end.getTime() - now.getTime();
      Logger.debug(
        `[${ctx.id} - ${command.constructor.name}] Finish execution. Took ${diff}ms`,
      );
    }
  }
}
