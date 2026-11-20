import { appConfig } from '@/config';
import { CooquoiClient } from '@utils/api-contracts/cooquoi';

const instance = new CooquoiClient(appConfig.backend.baseUrl);

export const cooquoiClient = instance;
