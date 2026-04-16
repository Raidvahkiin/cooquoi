import type { HealthEndpoints } from '../../interfaces';
import { HealthStatus } from '../../models/health';

export class HealthClient implements HealthEndpoints {
  constructor(private readonly baseUrl: string) {}

  async status(): Promise<HealthStatus> {
    const res = await fetch(`${this.baseUrl}/health`);
    const data = (await res.json()) as { status: string };
    return new HealthStatus(data.status);
  }
}
