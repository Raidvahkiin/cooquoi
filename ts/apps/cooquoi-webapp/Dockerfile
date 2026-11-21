# Node version must match .node-version
ARG NODE_IMAGE=node:24.15.0-alpine

# ---- Stage 1: Install dependencies ----
FROM ${NODE_IMAGE} AS deps

WORKDIR /app

# pnpm version comes from package.json "packageManager"
ENV COREPACK_ENABLE_DOWNLOAD_PROMPT=0
RUN corepack enable pnpm

# Copy full monorepo (filtered by .dockerignore)
COPY . .

RUN pnpm install --frozen-lockfile

# ---- Stage 2: Build ----
FROM ${NODE_IMAGE} AS builder

WORKDIR /app

ENV COREPACK_ENABLE_DOWNLOAD_PROMPT=0
RUN corepack enable pnpm

COPY --from=deps /app ./

ENV NEXT_TELEMETRY_DISABLED=1

RUN pnpm exec nx sync && pnpm exec nx build @cooquoi/webapp

# ---- Stage 3: Production image ----
FROM ${NODE_IMAGE} AS production

WORKDIR /app

ENV NODE_ENV=production
ENV NEXT_TELEMETRY_DISABLED=1
ENV PORT=3000

# Copy standalone server
COPY --from=builder /app/ts/apps/cooquoi-webapp/.next/standalone ./
# Copy static assets
COPY --from=builder /app/ts/apps/cooquoi-webapp/.next/static ./ts/apps/cooquoi-webapp/.next/static
# Copy public folder
COPY --from=builder /app/ts/apps/cooquoi-webapp/public ./ts/apps/cooquoi-webapp/public

EXPOSE 3000

CMD ["node", "ts/apps/cooquoi-webapp/server.js"]
