import { Type } from '@nestjs/common';
import { ICommand } from '@nestjs/cqrs';

/**
 * {@link CommandMiddlewareMetadata} metadata
 *
 * This interface defines the metadata for the {@link CommandMiddlewareMetadata} class.
 * It includes the hook type and the command type that the processor is associated with.
 */
export interface CommandMiddlewareMetadata<TCommand extends ICommand> {
  type: Type<TCommand>;
}
