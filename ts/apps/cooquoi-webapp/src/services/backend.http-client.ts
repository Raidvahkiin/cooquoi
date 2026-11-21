import { CooquoiClient } from '@utils/api-contracts/cooquoi';
import { appConfig } from '@/config';

const instance = new CooquoiClient(appConfig.backend.baseUrl);

export const cooquoiClient = instance;
