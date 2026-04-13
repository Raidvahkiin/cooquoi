import { Command } from '@nestjs/cqrs';

import { v7 as uuid } from 'uuid';

import {
  CommandMiddlewareContext,
  CommandMiddlewareNext,
  ICommandMiddlewareHandler,
} from '../middlewares';

import { CommandMiddlewareHandlerNotFoundError, buildChain } from './chain';

describe('buildChain', () => {
  class TestCommand extends Command<any> {}

  const createMiddleware = (
    id: string,
    calls: string[],
  ): ICommandMiddlewareHandler<TestCommand, string> => ({
    process: async (context, next) => {
      calls.push(id);
      const result = await next(context);
      return result + id;
    },
  });

  it('should chain middlewares in correct order', async () => {
    // Arrange
    const calls: string[] = [];

    const m1 = createMiddleware('A', calls);
    const m2 = createMiddleware('B', calls);
    const m3 = createMiddleware('C', calls);

    const final: CommandMiddlewareNext<TestCommand, string> = async () => 'X';

    // Act
    const chain = buildChain([m1, m2, m3], final);

    const context: CommandMiddlewareContext<TestCommand> = {
      id: uuid(),
      command: new TestCommand(),
    };

    const result = await chain(context);

    // Assert
    expect(result).toBe('X' + 'C' + 'B' + 'A');
    expect(calls).toEqual(['A', 'B', 'C']);
  });

  it('should throw if a middleware is missing', () => {
    // Arrange
    const m1 = undefined as any;

    // Act
    const doIt = () => buildChain([m1], final);

    const final: CommandMiddlewareNext<TestCommand, string> = async () => 'X';

    expect(doIt).toThrow(CommandMiddlewareHandlerNotFoundError);
  });
});
