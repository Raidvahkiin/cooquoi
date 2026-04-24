# ---- Stage 1: Install dependencies ----
FROM node:22-alpine AS deps

WORKDIR /app

RUN npm install -g bun

# Copy full monorepo (filtered by .dockerignore)
COPY . .

RUN bun install --frozen-lockfile

# ---- Stage 2: Build ----
FROM node:22-alpine AS builder

WORKDIR /app

RUN npm install -g bun

COPY --from=deps /app ./

ENV NEXT_TELEMETRY_DISABLED=1

RUN bun nx sync && bun nx build @cooquoi/webapp

# ---- Stage 3: Production image ----
FROM node:22-alpine AS production

WORKDIR /app

ENV NODE_ENV=production
ENV NEXT_TELEMETRY_DISABLED=1
ENV PORT=3000

# Copy standalone server
COPY --from=builder /app/apps/cooquoi-webapp/.next/standalone ./
# Copy static assets
COPY --from=builder /app/apps/cooquoi-webapp/.next/static ./apps/cooquoi-webapp/.next/static
# Copy public folder
COPY --from=builder /app/apps/cooquoi-webapp/public ./apps/cooquoi-webapp/public

EXPOSE 3000

CMD ["node", "apps/cooquoi-webapp/server.js"]
