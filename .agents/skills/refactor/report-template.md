# Refactor report

## Scope

- **Level 0** (changed by the task): `…`
- **Level 1**: `…`
- **Level 2**: `…`
- **Crossed boundaries** (shared / other context — dead code or pass-through only): none | `…`

## Smells fixed

| File | Smell | Fix |
|---|---|---|
| `…` | `…` | `…` |

## Smells skipped

| File | Smell | Allowed reason (1–4) | Follow-up |
|---|---|---|---|
| `…` | `…` | `…` | `…` |

## Verification loop

| Pass | Dead code removed | Pass-throughs inlined | Speculative layers removed | Other leftovers |
|---|---|---|---|---|
| 1 | `…` | `…` | `…` | `…` |
| 2 | `…` | `…` | `…` | `…` |
| 3 | — | — | — | — |

Remaining after pass 3: none | `…`

## Checks

| Command | Result |
|---|---|
| `pnpm nx affected -t lint` | ✅ / ❌ |
| `pnpm nx affected -t typecheck` | ✅ / ❌ |
| `pnpm nx affected -t test` | ✅ / ❌ |
| `pnpm biome check` | ✅ / ❌ |
| `pnpm knip` (or `dotnet build` with analyzers) | ✅ / ❌ |
