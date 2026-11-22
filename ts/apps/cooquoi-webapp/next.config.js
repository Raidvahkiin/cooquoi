//@ts-check
const path = require('node:path');
const createNextIntlPlugin = require('next-intl/plugin');

const withNextIntl = createNextIntlPlugin('./src/shared/i18n/request.ts');

// Monorepo root: standalone output and Turbopack resolve workspace packages from here,
// so the server ends up at .next/standalone/ts/apps/cooquoi-webapp/server.js (see Dockerfile).
const workspaceRoot = path.join(__dirname, '../../..');

/** @type {import('next').NextConfig} */
const nextConfig = {
  output: 'standalone',
  outputFileTracingRoot: workspaceRoot,
  turbopack: { root: workspaceRoot },
  transpilePackages: ['@cooquoi/api-client'],
  // Loaded by Node at runtime, not bundled: @redis/client pulls in a native addon (@node-rs/xxhash).
  // The proxy runs on Node.js (Next 16), so it can use them too.
  serverExternalPackages: ['redis', '@redis/client'],
};

module.exports = withNextIntl(nextConfig);
