import { Controller, Get } from '@nestjs/common';
import {
  HealthEndpoints,
  HealthStatus,
  HealthStatusResponse,
} from '@utils/api-contracts/cooquoi';

@Controller('health')
export class HealthController implements HealthEndpoints {
  @Get()
  async status(): Promise<HealthStatusResponse> {
    return { status: HealthStatus.OK.value };
  }
}
