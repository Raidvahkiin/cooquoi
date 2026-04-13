import { Test } from '@nestjs/testing';
import { Command, CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { ConsoleLogger, LoggerService } from '@nestjs/common';

import { MediatorModule } from './module';
import {
  ICommandMiddlewareHandler,
  CommandMiddlewareContext,
  CommandMiddlewareNext,
} from './middlewares';
import { CommandMiddleware } from './decorators';

class TestCommand extends Command<void> {}

@CommandMiddleware(Command)
class TestCommandMiddleware
  implements ICommandMiddlewareHandler<Command<void>, void>
{
  private readonly logger: LoggerService;

  constructor() {
    this.logger = new ConsoleLogger(TestCommandMiddleware.name);
  }

  async process(
    ctx: CommandMiddlewareContext<Command<void>>,
    next: CommandMiddlewareNext<Command<void>, void>,
  ): Promise<void> {
    const command = ctx.command;

    const now = new Date();

    const result = await next(ctx);

    const end = new Date();

    const diff = end.getTime() - now.getTime();

    this.logger.log(
      `Processing command ${command.constructor.name}[${ctx.id}] took ${diff}ms`,
    );

    return result;
  }
}

@CommandHandler(TestCommand)
class TestCommandHandler implements ICommandHandler<TestCommand> {
  private readonly logger: LoggerService;

  constructor() {
    this.logger = new ConsoleLogger(TestCommandHandler.name);
  }

  async execute(command: TestCommand): Promise<void> {
    this.logger.log(`Executing command ${command.constructor.name}`);
    return Promise.resolve();
  }
}

describe('Command Pipeline', () => {
  const testModuleBuilder = Test.createTestingModule({
    imports: [
      MediatorModule.forRoot({
        middlewares: [TestCommandMiddleware],
      }),
    ],
    providers: [TestCommandHandler],
  });

  it('should execute command with pipelines', async () => {
    const testModule = await testModuleBuilder.compile();

    await testModule.init();

    const bus = testModule.get(MediatorModule.getCommandBusToken());

    await bus.execute(new TestCommand());
  });
});
