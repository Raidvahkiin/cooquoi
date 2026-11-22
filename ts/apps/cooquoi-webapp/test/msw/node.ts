import { setupServer } from 'msw/node';
import { householdHandlers } from './handlers/household';
import { identityHandlers } from './handlers/identity';

export const server = setupServer(...identityHandlers, ...householdHandlers);
