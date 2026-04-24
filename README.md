# Cooquoi Monorepo

## Docker

### Build the API image

```bash
nx docker-build @cooquoi/api --args="--tag=latest"
```

Or directly:

```bash
docker build -f apps/cooquoi-api/Dockerfile -t cooquoi-api:latest .
```

### Run the API container

```bash
docker run -p 3001:3001 -e DATABASE_URL=postgres://user:pass@host:5432/cooquoi cooquoi-api:latest
```

### Environment variables

| Variable | Default | Description |
|---|---|---|
| `DATABASE_URL` | *(empty)* | PostgreSQL connection string (required) |
| `PORT` | `3001` | HTTP listen port |
| `DATABASE_SEEDING_ENABLED` | `false` | Seed database on startup |
