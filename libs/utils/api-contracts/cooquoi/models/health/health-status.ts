export class HealthStatus {
  static OK = new HealthStatus('ok');
  static ERROR = new HealthStatus('error');

  constructor(public value: string) {}

  toString(): string {
    return this.value;
  }

  toJSON(): string {
    return this.value;
  }
}

export type HealthStatusResponse = {
  status: HealthStatus;
};
