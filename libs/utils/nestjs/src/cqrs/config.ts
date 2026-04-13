import { ConfigurableModuleBuilder, Type } from '@nestjs/common';

import { CqrsModuleOptions } from '@nestjs/cqrs';

import { _ICommandMiddlewareHandler } from './middlewares';

export const { OPTIONS_TYPE, MODULE_OPTIONS_TOKEN, ConfigurableModuleClass } =
  new ConfigurableModuleBuilder<{
    /**
     * Options for the CQRS module {@link CqrsModuleOptions}
     */
    cqrs?: CqrsModuleOptions;
    /**
     * List of command middlewares {@link _ICommandMiddlewareHandler}
     */
    middlewares: Type<_ICommandMiddlewareHandler>[];
  }>()
    .setClassMethodName('forRoot')
    .build();
