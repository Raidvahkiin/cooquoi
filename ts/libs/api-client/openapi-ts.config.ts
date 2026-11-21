import { defineConfig } from '@hey-api/openapi-ts';

export default defineConfig({
  input: '../../../contracts/generated/openapi.yaml',
  output: 'src/generated',
  plugins: ['@hey-api/client-fetch', '@hey-api/sdk', '@hey-api/typescript'],
});
