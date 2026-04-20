import { Command, ICommand } from '@nestjs/cqrs';

/**
 * {@link CommandMiddlewareNext} type
 *
 * This type represents the next function in the command middleware pipeline.
 */
export type CommandMiddlewareNext<
  TCommand extends Command<TCommandResult>,
  TCommandResult,
> = (ctx: CommandMiddlewareContext<TCommand>) => Promise<TCommandResult>;

export interface CommandMiddlewareContext<TCommand extends ICommand> {
  readonly id: string;
  readonly command: TCommand;
}

/**
 * {@link _ICommandMiddlewareHandler} interface
 *
 * This interface defines the structure of a command middleware handler.
 */
export type _ICommandMiddlewareHandler = object;

/**
 * {@link ICommandMiddlewareHandler} class
 *
 * This class is an abstract base class for command pipeline processors.
 * It defines the process method that must be implemented by subclasses to handle the processing of commands.
 */
export interface ICommandMiddlewareHandler<
  TCommand extends Command<TCommandResult>,
  TCommandResult,
> extends _ICommandMiddlewareHandler {
  /**
   * The process method is called to process the command.
   *
   * @param command The command to be processed
   * @param next The next middleware in the chain
   */
  process(
    ctx: CommandMiddlewareContext<TCommand>,
    next: CommandMiddlewareNext<TCommand, TCommandResult>,
  ): Promise<TCommandResult>;
}
