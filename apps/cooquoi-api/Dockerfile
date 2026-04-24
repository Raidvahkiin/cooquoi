# ---- Stage 1: Install dependencies ----
FROM oven/bun:1 AS deps

WORKDIR /app

# Copy full monorepo (filtered by .dockerignore)
# Lockfile needs all workspace package.json files for resolution
COPY . .

RUN bun install --frozen-lockfile --production

# ---- Stage 2: Production image ----
FROM oven/bun:1-slim AS production

WORKDIR /app

# Copy installed node_modules from deps stage
COPY --from=deps /app/node_modules ./node_modules

# Copy workspace root config
COPY package.json ./
COPY tsconfig.base.json ./
COPY tsconfig.json ./

# Copy app source + default .env
COPY apps/cooquoi-api/ ./apps/cooquoi-api/

# Copy workspace lib sources (Bun runs .ts natively)
COPY libs/contexts/market/ ./libs/contexts/market/
COPY libs/utils/nestjs/ ./libs/utils/nestjs/
COPY libs/utils/api-contracts/ ./libs/utils/api-contracts/
COPY libs/core/ ./libs/core/

ENV NODE_ENV=production
ENV PORT=3001

EXPOSE 3001

CMD ["bun", "run", "apps/cooquoi-api/src/main.ts"]
