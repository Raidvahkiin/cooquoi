export abstract class ErrorBase extends Error {
  constructor(
    public readonly code: string,
    message: string,
    options?: ErrorOptions,
  ) {
    super(`[ERROR] ${code}: ${message}`, options);
  }
}
