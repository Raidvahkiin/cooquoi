import { Logger } from '@nestjs/common';
import { Command } from '@nestjs/cqrs';
import { CommandMiddleware, EnableLoggingDecorator } from '../decorators';

import {
  CommandMiddlewareContext,
  CommandMiddlewareNext,
  ICommandMiddlewareHandler,
} from './command-handler.middleware';

function maskSensitiveData(obj: any, sensitiveKeys: string[]) {
  if (typeof obj === 'object' && obj !== null) {
    for (const key in obj) {
      if (Object.hasOwn(obj, key)) {
        if (
          sensitiveKeys.map((k) => k.toLowerCase()).includes(key.toLowerCase())
        ) {
          obj[key] = '***';
        } else if (typeof obj[key] === 'object' && obj[key] !== null) {
          maskSensitiveData(obj[key], sensitiveKeys);
        }
      }
    }
  }
  return obj;
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
      const commandDetail = JSON.stringify(
        maskSensitiveData(
          structuredClone(command),
          EnableLoggingDecorator.getConfigFrom(command).ignore ?? [],
        ),
      );
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
