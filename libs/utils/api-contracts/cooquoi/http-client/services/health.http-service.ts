import type { HealthEndpoints } from '../../interfaces';
import { HealthStatus, HealthStatusResponse } from '../../models/health';

export class HealthClient implements HealthEndpoints {
  constructor(private readonly baseUrl: string) {}

  async status(): Promise<HealthStatusResponse> {
    const res = await fetch(`${this.baseUrl}/health`);
    const data = (await res.json()) as { status: string };
    return { status: new HealthStatus(data.status) };
  }
}
