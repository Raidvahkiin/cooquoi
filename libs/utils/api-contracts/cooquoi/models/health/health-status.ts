export class HealthStatus {
  static readonly instances: HealthStatus[] = [];
  static OK = new HealthStatus('ok');
  static ERROR = new HealthStatus('error');

  constructor(public readonly value: string) {
    HealthStatus.instances.push(this);
  }

  toString(): string {
    return this.value;
  }

  toJSON(): string {
    return this.value;
  }

  static fromString(value: string): HealthStatus {
    const status = HealthStatus.instances.find((s) => s.value === value);
    if (!status) {
      throw new Error(`Invalid health status: ${value}`);
    }
    return status;
  }
}

export type HealthStatusResponse = {
  status: string;
};
