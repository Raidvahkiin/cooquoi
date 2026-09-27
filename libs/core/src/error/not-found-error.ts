import { ErrorBase } from './error-base';

export class NotFoundError extends ErrorBase {
  constructor(
    public readonly target: string,
    public readonly findBy: string,
    public readonly findValue: string,
    code: string,
    message?: string,
    options?: ErrorOptions,
  ) {
    super(
      code,
      `{${target}} Not Found: find by '${findBy}' = ${findValue}. ${message ?? ''}`,
      options,
    );
  }
}
