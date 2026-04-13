import { Injectable, OnModuleInit, Scope } from '@nestjs/common';
import { ModulesContainer } from '@nestjs/core';
import { InstanceWrapper } from '@nestjs/core/injector/instance-wrapper';
import { Command } from '@nestjs/cqrs';
import { v7 as uuid } from 'uuid';
import {
  CommandMiddlewareContext,
  _ICommandMiddlewareHandler,
} from '../middlewares';
import { filterMiddlewares, getMiddlewares } from '../reflection';

import { buildChain } from './chain';

/**
 * Symbol for the MediatorPip class
 * This symbol is used to identify the MediatorPip class in the NestJS DI system.
 */
export const ICommandPipeline = Symbol('CommandPipeline');

/**
 * {@link ICommandPipeline} interface
 *
 * This interface defines the contract for the {@link CommandPipeline} class.
 */
export interface ICommandPipeline {
  /**
   *
   * @param command The command to be processed by the command handler
   * @param resolve The callback function to execute the command handler
   */
  pipe<TCommand extends Command<TCommandResult>, TCommandResult>(
    command: TCommand,
    resolve: (command: TCommand) => Promise<TCommandResult>,
  ): Promise<TCommandResult>;
}

/**
 * {@link CommandPipeline} class
 *
 * This class implements the {@link ICommandPipeline} interface and provides the functionality to pipe commands through the middlewares.
 */
@Injectable({
  scope: Scope.DEFAULT,
})
export class CommandPipeline implements OnModuleInit, ICommandPipeline {
  /**
   * Array of middlewares to be used in the command pipeline
   * This array is populated by the {@link onModuleInit} method when the module is initialized.
   */
  private readonly middlewares: InstanceWrapper<_ICommandMiddlewareHandler>[] =
    [];

  constructor(private readonly modulesContainer: ModulesContainer) {}

  async pipe<TCommand extends Command<TCommandResult>, TCommandResult>(
    command: TCommand,
    execute: (command: TCommand) => Promise<TCommandResult>,
  ): Promise<TCommandResult> {
    // Create the initial context
    const id = uuid().slice(0, 8);

    const ctx: CommandMiddlewareContext<TCommand> = {
      id,
      command,
    };

    const middlewares = filterMiddlewares<TCommand, TCommandResult>(
      command,
      this.middlewares,
    );

    return buildChain(
      middlewares,
      (context: CommandMiddlewareContext<TCommand>) => {
        return execute(context.command);
      },
    )(ctx);
  }

  /**
   *
   * Lifecycle hook that is called when the module is initialized.
   * This method is used to scan the modules container for all providers and register them as middlewares.
   *
   * This method is called automatically by NestJS when the module is initialized and is inspired by the CqrsModule from the NestJS CQRS module.
   */
  onModuleInit() {
    this.middlewares.push(...getMiddlewares(this.modulesContainer));
  }
}
