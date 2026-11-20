import { HealthStatus } from '../../models/health';

export class HealthClient {
  constructor(private readonly baseUrl: string) {}

  async status(): Promise<HealthStatus> {
    const res = await fetch(`${this.baseUrl}/health`);
    const data = (await res.json()) as { status: string };
    return HealthStatus.fromString(data.status);
  }
}
