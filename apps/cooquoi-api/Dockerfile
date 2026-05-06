# ---- Stage 1: Install dependencies ----
FROM oven/bun:1 AS deps

WORKDIR /app

# Copy full monorepo (filtered by .dockerignore)
# Lockfile needs all workspace package.json files for resolution
COPY . .

RUN bun install --frozen-lockfile

# ---- Stage 2: Production image ----
FROM oven/bun:1-slim AS production

WORKDIR /app

# Copy everything from deps (node_modules at all levels + source)
COPY --from=deps /app ./

ENV NODE_ENV=production
ENV PORT=3001

EXPOSE 3001

CMD ["bun", "run", "apps/cooquoi-api/src/main.ts"]
