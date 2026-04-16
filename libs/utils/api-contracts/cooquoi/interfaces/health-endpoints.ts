import { HealthStatusResponse } from '../models/health';

export interface HealthEndpoints {
  status(): Promise<HealthStatusResponse>;
}
