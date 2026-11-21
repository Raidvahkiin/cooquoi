# Node version must match .node-version.
# Debian slim (not alpine) keeps bash available for the Coolify terminal.
ARG NODE_IMAGE=node:24.15.0-bookworm-slim

# ---- Stage 1: Install dependencies ----
FROM ${NODE_IMAGE} AS deps

WORKDIR /app

# pnpm version comes from package.json "packageManager"
ENV COREPACK_ENABLE_DOWNLOAD_PROMPT=0
RUN corepack enable pnpm

# Copy full monorepo (filtered by .dockerignore)
# The lockfile needs every workspace package.json for resolution
COPY . .

RUN pnpm install --frozen-lockfile


# ---- Stage 2: Production image ----
FROM ${NODE_IMAGE} AS production

WORKDIR /app

ENV COREPACK_ENABLE_DOWNLOAD_PROMPT=0
RUN corepack enable pnpm

# Copy everything from deps (node_modules at all levels + source)
COPY --from=deps /app ./

ENV NODE_ENV=production
ENV PORT=3001

EXPOSE 3001

# Run migrations, then start the API on Node (TypeScript compiled on the fly by @swc-node/register)
CMD ["sh", "-c", "cd /app/apps/cooquoi-api && pnpm run db:migrate && pnpm run start"]
