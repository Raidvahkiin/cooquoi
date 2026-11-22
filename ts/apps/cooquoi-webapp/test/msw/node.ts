import { setupServer } from 'msw/node';
import { foodpediaHandlers } from './handlers/foodpedia';
import { householdHandlers } from './handlers/household';
import { identityHandlers } from './handlers/identity';

export const server = setupServer(
  ...identityHandlers,
  ...householdHandlers,
  ...foodpediaHandlers,
);
