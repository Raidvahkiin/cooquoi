# Market Context (`@cooquoi/market`)

Handles ingredients, products, vendor price offers, and price comparison.

> **Domain model** (aggregates, entities, value objects, ER diagram, invariants): [docs/domain-model.md](docs/domain-model.md)

---

## Architecture

### Vertical Slice + Clean Architecture

Each feature is a self-contained **vertical slice** — one folder per use case under `src/features/`. A slice owns everything it needs: command/query, handler, and any local types. Nothing leaks between slices.

```
src/
  domain/           # Drizzle tables, relations, value objects
    entities/       # *.entity.ts — pgTable definitions + inferred types
    value-objects/  # *.vo.ts — validated immutable types (e.g. Price)
  features/
    crud-ingredients/
      create-ingredient/
        create-ingredient.command.ts
        create-ingredient.handler.ts
        create-ingredient.dto.ts
      delete-ingredient/
      get-ingredient/
      filter-ingredients/
    crud-products/
      ...
    crud-offers/
      ...
    seeding/
  config/           # DATABASE_TOKEN, ModuleConfig schema (zod)
  market.module.ts  # DynamicModule — only entry point for consumers
  index.ts          # Public barrel — only export what consumers need
```

**Rule**: controllers are **not** part of this library. They live in `apps/cooquoi-api/src/controllers/` and import commands, queries, and result types from the public barrel.

### Domain layer

> Full UML class diagram and ER diagram: [docs/domain-model.md](docs/domain-model.md)

Domain entities are **Drizzle table definitions** — they serve as both schema and data model. No separate ORM entity class or repository interface.

```ts
// domain/entities/ingredient.aggregate.ts
export const ingredients = pgTable('ingredients', {
  id: uuid('id').primaryKey().defaultRandom(),
  name: varchar('name', { length: 255 }).notNull().unique(),
  ...
});

export type Ingredient = typeof ingredients.$inferSelect;
```

Value objects encapsulate invariants and are validated at construction:

```ts
// domain/value-objects/price.vo.ts
const price = Price.create({ amount: 9.99, currency: 'EUR' }); // throws if invalid
```

---

## CQRS

Every use case is either a **Command** (mutates state, returns a result) or a **Query** (reads state, never mutates).

### Command

```ts
// create-ingredient.command.ts
@EnableLogging()
export class CreateIngredientCommand extends Command<Ingredient> {
  constructor(
    public readonly name: string,
    public readonly description?: string,
  ) {
    super();
  }
}

// create-ingredient.handler.ts
@CommandHandler(CreateIngredientCommand)
export class CreateIngredientHandler
  implements ICommandHandler<CreateIngredientCommand, Ingredient>
{
  constructor(@Inject(DATABASE_TOKEN) private readonly db: MarketDatabase) {}

  async execute(command: CreateIngredientCommand): Promise<Ingredient> {
    const [row] = await this.db
      .insert(ingredients)
      .values({ name: command.name, description: command.description })
      .returning();
    return row;
  }
}
```

### Query

```ts
// get-ingredient.query.ts
export class GetIngredientQuery extends Query<Ingredient | null> {
  constructor(public readonly id: string) { super(); }
}

// get-ingredient.handler.ts
@QueryHandler(GetIngredientQuery)
export class GetIngredientHandler
  implements IQueryHandler<GetIngredientQuery, Ingredient | null>
{
  constructor(@Inject(DATABASE_TOKEN) private readonly db: MarketDatabase) {}

  async execute(query: GetIngredientQuery): Promise<Ingredient | null> {
    const [row] = await this.db
      .select()
      .from(ingredients)
      .where(eq(ingredients.id, query.id));
    return row ?? null;
  }
}
```

### Registering handlers

Add new handlers to the `commandHandlers` or `queryHandlers` arrays in `market.module.ts`. The module's `static register()` factory registers them as NestJS providers automatically.

---

## Database setup

The context uses **Drizzle ORM** with `node-postgres`. Configuration is injected via `ModuleConfig` — validated with Zod at startup.

### Adding/changing schema

1. Edit or create a table file in `src/domain/entities/*.entity.ts`.
2. Update `src/domain/entities/relations.ts` if relations change.
3. Re-export from `src/domain/index.ts` if it's a new file.
4. Generate a migration:

```bash
# from libs/contexts/market/
bun drizzle-kit generate --config drizzle-test.config.ts
```

> The test migration output goes to `src/__tests__/setup/drizzle/` and is used by PGlite during tests. For the app-level migration, use `apps/cooquoi-api/drizzle.config.ts`.

### Wiring in the app

```ts
MarketModule.register({ database: { url: process.env.DATABASE_URL } })
```

---

## Testing

Tests are **integration tests** — they run real SQL against an in-memory **PGlite** database (no mocks for the DB layer).

### Test infrastructure

| File | Purpose |
|---|---|
| `src/__tests__/setup/pglite.ts` | Spins up a PGlite in-memory DB and runs migrations |
| `src/__tests__/setup/index.ts` | Creates a full NestJS test module with `DATABASE_TOKEN` overridden to PGlite; exposes `commandBus`, `queryBus`, `resetDb`, `clearDb` |
| `src/__tests__/setup/drizzle/` | Migration snapshots generated by `drizzle-kit generate --config drizzle-test.config.ts` |

### Writing a test

Place spec files next to the feature they test: `src/features/<slice>/__tests__/<slice>.spec.ts`.

```ts
import { getTestSuit } from '../../../../__tests__/setup';
import { CreateIngredientCommand } from '../create-ingredient.command';

describe('CreateIngredient feature', () => {
  let testSuit: Awaited<ReturnType<typeof getTestSuit>>;

  beforeAll(async () => { testSuit = await getTestSuit(); });
  afterAll(async () => { await testSuit?.app?.close(); });
  beforeEach(async () => { await testSuit.resetDb(); });

  it('should create an ingredient', async () => {
    const { commandBus } = testSuit;

    const ingredient = await commandBus.execute(
      new CreateIngredientCommand('garlic', 'aromatic herb'),
    );

    expect(ingredient).toMatchObject({ name: 'garlic' });
  });
});
```

**Seed data** (from `resetDb`): `salt`, `shallot`, `salmon`, `pepper` ingredients; `Super Salmon`, `Greater Salmon`, `Spicy Pepper` products with offers from VendorA/B/C.

### Running tests

```bash
bun nx run market:test
```

---

## Clean code conventions

- **Parameter objects**: commands and functions with ≥ 3 parameters take a single typed object argument.
- **File naming**: `*.command.ts`, `*.handler.ts`, `*.query.ts`, `*.vo.ts`, `*.entity.ts`, `*.spec.ts`.
- **No barrel re-export of internals**: `src/index.ts` exports only what app-level code needs (commands, queries, DTOs, result types, `MarketModule`).
- **No controllers inside the library**: dispatch happens from `apps/cooquoi-api/src/controllers/` via `CommandBus` / `QueryBus`.
- **Handlers are thin**: business logic belongs in domain methods or value objects, not in handlers.
- **Biome** enforces formatting (2-space indent, single quotes). Run `bun nx run market:lint` before committing.
