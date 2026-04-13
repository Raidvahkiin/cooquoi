import { InstanceWrapper } from '@nestjs/core/injector/instance-wrapper';

import { ModulesContainer } from '@nestjs/core';

import { Command } from '@nestjs/cqrs';

import {
  ICommandMiddlewareHandler,
  _ICommandMiddlewareHandler,
} from './middlewares';
import {
  getMiddlewareMetadata,
  middlewareMetadataExists,
} from './decorators/decorator.reflection';

/**
 * This function retrieves all the middlewares registered in the modules container.
 *
 * @param modulesContainer The modules container from NestJS
 *
 * @returns The list of middlewares registered in the modules container
 */
export const getMiddlewares = (modulesContainer: ModulesContainer) => {
  const modules = [...modulesContainer.values()];

  // Get all providers from the modules container
  const providers = modules.flatMap((module) => {
    const providers = module.providers.values();
    return providers ? [...providers] : [];
  });

  // Filter the providers to find the ones that are command pipeline processors
  const middlewares = providers
    .map((provider) => {
      const instance = provider.instance;

      if (!instance || !middlewareMetadataExists(instance.constructor)) {
        return undefined;
      }

      return provider;
    })
    .filter((p): p is InstanceWrapper<_ICommandMiddlewareHandler> => !!p);

  return middlewares;
};

/**
 * Filters the list of middlewares based on the command type.
 *
 * @param command The command to be processed
 * @param middlewares The list of processors to filter
 * @returns The filtered list of processors that can process the command
 */
export const filterMiddlewares = <
  TCommand extends Command<TCommandResult>,
  TCommandResult,
>(
  command: TCommand,
  middlewares: InstanceWrapper<_ICommandMiddlewareHandler>[],
) => {
  return middlewares
    .filter((middleware) => {
      const metadata = getMiddlewareMetadata(middleware);

      return command instanceof metadata.type;
    })
    .map((middleware) => {
      return middleware.instance as ICommandMiddlewareHandler<
        TCommand,
        TCommandResult
      >;
    });
};
