const EnableLoggingDecoratorMetadata = Symbol('EnableLoggingDecoratorMetadata');

export interface EnableLoggingConfig {
  ignore?: string[];
}

export function EnableLogging(config?: EnableLoggingConfig): ClassDecorator {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  return (target: any) => {
    Reflect.defineMetadata(
      EnableLoggingDecoratorMetadata,
      config ?? {},
      target,
    );
  };
}

export const EnableLoggingDecorator = {
  existIn: (target: object): boolean => {
    return Reflect.getMetadataKeys(target.constructor).includes(
      EnableLoggingDecoratorMetadata,
    );
  },
  getConfigFrom: (target: object): EnableLoggingConfig => {
    return (
      Reflect.getMetadata(EnableLoggingDecoratorMetadata, target.constructor) ??
      {}
    );
  },
};
