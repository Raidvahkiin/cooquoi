# Copilot code guiline

## 1. Monorepo structure

This is an Nx-powered monorepo using pnpm workspaces. The structure follows a clear separation between applications and libraries.

### Workspace Overview

- **Package Manager**: pnpm (version 10.27.0+)
- **Build System**: Nx (version 22.x)
- **Workspace Root**: `/home/trung/Dev/cooquoi`

### Directory Structure

```
/
├── apps/           # All application projects
│   ├── cooquoi-api-admin/      # NestJS backend API
│   ├── cooquoi-webapp/         # Next.js frontend application
│   └── cooquoi-webapp-e2e/     # Playwright E2E tests
├── libs/           # All library projects
│   ├── cooquoi-domain/         # Domain layer (DDD)
│   ├── cooquoi-application/    # Application layer (DDD)
│   ├── cooquoi-infrastructure/ # Infrastructure layer (DDD)
│   ├── cooquoi-presentation/   # Presentation layer (DDD)
│   ├── core/                   # Core utilities (DDD base classes)
│   ├── utils-mongoose/         # Mongoose utilities
│   └── utils-nestjs/           # NestJS utilities
├── docker/         # Docker compose files
├── scripts/        # Build and utility scripts
└── [config files]  # Root configuration files
```

### Apps Directory (`apps/`)

Contains all runnable applications:

- **NestJS Applications**: Backend API services (e.g., `cooquoi-api-admin`)
- **Next.js Applications**: Frontend web applications (e.g., `cooquoi-webapp`)
- **E2E Test Suites**: Playwright test applications (e.g., `cooquoi-webapp-e2e`)

### Libs Directory (`libs/`)

Contains all shared libraries organized by:

1. **Domain Layers** (following DDD):
   - `cooquoi-domain`: Entities, value objects, domain errors
   - `cooquoi-application`: Commands, command handlers, use cases
   - `cooquoi-infrastructure`: Database implementations, external services
   - `cooquoi-presentation`: Queries, DTOs, view models

2. **Core/Utility Libraries**:
   - `core`: Base DDD classes, common utilities, datetime helpers
   - `utils-mongoose`: Mongoose-specific utilities and filters
   - `utils-nestjs`: NestJS-specific utilities and modules

### Package Naming Convention

- **Applications**: `@cooquoi/<app-name>` (e.g., `@cooquoi/api-admin`, `@cooquoi/webapp`)
- **Domain Libraries**: `@cooquoi/<layer>` (e.g., `@cooquoi/domain`, `@cooquoi/application`)
- **Utility Libraries**: `@libs/<name>` (e.g., `@libs/core`, `@libs/utils-nestjs`)

### Referencing Projects

#### In package.json

Use `workspace:*` protocol to reference workspace packages:

```json
{
  "dependencies": {
    "@cooquoi/domain": "workspace:*",
    "@cooquoi/application": "workspace:*",
    "@libs/core": "workspace:*"
  }
}
```

#### In TypeScript Code

Import directly using package names (no relative paths needed):

```typescript
// ✅ Correct - use package names
import { Entity } from '@libs/core';
import { Ingredient } from '@cooquoi/domain';
import { CreateIngredientCommand } from '@cooquoi/application';

// ❌ Wrong - avoid relative paths between packages
import { Entity } from '../../../libs/core/src';
```

#### TypeScript Project References

Each project uses TypeScript project references in `tsconfig.json`:

```jsonc
{
  "extends": "../../tsconfig.base.json",
  "references": [
    { "path": "../../libs/cooquoi-domain" },
    { "path": "../../libs/core" }
  ]
}
```

### Project Tags (Nx)

Projects are tagged for dependency constraints and organization:

- **scope**: `cooquoi`, `core`
- **type**: `app`, `lib`
- **layer** (for libs): `domain`, `application`, `infrastructure`, `presentation`, `standalone`
- **platform**: `frontend`, `backend`, `shared`

Example from `package.json`:
```json
{
  "nx": {
    "tags": ["scope:cooquoi", "type:lib", "layer:domain", "platform:shared"]
  }
}
```

### Build and Task Execution

All projects are connected via Nx dependency graph. Build commands automatically handle dependencies:

```bash
# Build specific project (automatically builds dependencies)
pnpm nx build @cooquoi/api-admin

# Run development server
pnpm nx dev @cooquoi/webapp

# Run tests for a project
pnpm nx test @cooquoi/domain

# Type-check all projects
pnpm nx run-many --target=typecheck

# Lint all projects
pnpm nx run-many --target=lint
```

### Adding New Projects

#### New Application
Place in `apps/` directory with package name `@cooquoi/<app-name>`

#### New Library
- Domain-specific: Place in `libs/` with name pattern `cooquoi-<layer>`
- Utility library: Place in `libs/` with name pattern `utils-<technology>` or descriptive name
- Use package name `@cooquoi/<name>` for domain libs, `@libs/<name>` for utilities

### Key Configuration Files

- `pnpm-workspace.yaml`: Defines workspace packages
- `nx.json`: Nx configuration, task pipelines, caching
- `tsconfig.base.json`: Base TypeScript configuration
- Individual `tsconfig.json`: Per-project TypeScript config with project references
- Individual `package.json`: Per-project dependencies using `workspace:*`

## 2. Domain-Driven Design (DDD)

This project follows Clean Architecture principles with a strict layered DDD approach. The architecture enforces clear dependency rules to maintain separation of concerns and ensure business logic independence.

### Architecture Layers

```
┌─────────────────────────────────────────────┐
│           Apps (cooquoi-api-admin)          │  ← Entry points
│     Depends on: Presentation ONLY           │
└─────────────────────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────┐
│      Presentation (@cooquoi/presentation)   │  ← Gateway/Facade
│  Depends on: Application, Domain, Infra     │
│  Re-exports: Commands, Queries, Interface   │
└─────────────────────────────────────────────┘
                      ↓
┌──────────────────┬──────────────────────────┐
│  Application     │    Infrastructure        │
│  (@cooquoi/      │  (@cooquoi/              │
│   application)   │   infrastructure)        │
│  Commands        │  DB Models, Repos        │
│  Depends: Domain │  Depends: Application,   │
│                  │           Domain          │
└──────────────────┴──────────────────────────┘
                      ↓
┌─────────────────────────────────────────────┐
│          Domain (@cooquoi/domain)           │  ← Core business logic
│  Entities, Value Objects, Interfaces        │
│  Depends on: Core only                      │
└─────────────────────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────┐
│            Core (@libs/core)                │  ← Base abstractions
│  DDD base classes, utilities                │
└─────────────────────────────────────────────┘
```

### Dependency Rules (STRICT)

**Critical: Dependencies must ONLY flow inward. Outer layers depend on inner layers, never the reverse.**

**Apps interact with ALL internal layers through the Presentation gateway ONLY.**

#### ✅ Allowed Dependencies

| Layer | Can Import From | Cannot Import From |
|-------|----------------|-------------------|
| **Apps** | **Presentation ONLY**, Utilities | ❌ Infrastructure, ❌ Application layer, ❌ Domain |
| **Presentation** | Application layer, Domain, Infrastructure, Core | Nothing (outer layer) |
| **Application layer** | Domain, Core | ❌ Infrastructure, ❌ Presentation |
| **Infrastructure** | Application layer, Domain, Core | ❌ Presentation |
| **Domain** | Core only | ❌ Application layer, ❌ Infrastructure, ❌ Presentation |
| **Core** | None (standalone) | Everything else |

#### ❌ Forbidden Patterns
ANY internal layer directly
import { IngredientModel } from "@cooquoi/infrastructure";     // NEVER
import { SeedDb } from "@cooquoi/infrastructure";               // NEVER
import { CreateIngredientCommand } from "@cooquoi/application"; // NEVER
import { Ingredient } from "@cooquoi/domain";                   // NEVER

// ✅ CORRECT: Apps ONLY import from Presentation
import { DbSeedable } from "@cooquoi/presentation";            // ✅ Re-exported
import { CreateIngredientCommand } from "@cooquoi/presentation"; // ✅ Re-exported
import { Ingredient } from "@cooquoi/presentation";             // ✅ Re-exported

// ❌ WRONG: Application layer importing Infrastructure
// In application layer command handler:
import { TypeOrmIngredientRepository } from "@cooquoi/infrastructure";  // NEVER

// ✅ CORRECT: Application layer using Domain interfaces
import { IngredientRepository } from "@cooquoi/domain";  // Use interface

// ❌ WRONG: Domain importing Application layerooquoi/domain";  // Use interface

// ❌ WRONG: Domain importing Application
// In domain entity:
import { CreateIngredientCommand } from "@cooquoi/application";  // NEVER
```

### Layer Responsibilities

#### 1. Domain Layer (`@cooquoi/domain`)

**Purpose**: Pure business logic, technology-agnostic

**Contains**:
- **Entities**: Business objects with identity (`Ingredient`, `Recipe`)
- **Value Objects**: Immutable objects without identity (`Money`, `Email`)
- **Domain Interfaces**: Contracts for repositories, services (`IngredientRepository`)
- **Domain Events**: Business event definitions
- **Domain Errors**: Business rule violation exceptions
- **Domain Constants**: Business constants

**Structure**:
```
libs/cooquoi-domain/src/
├── entities/              # Aggregate roots and entities
│   └── ingredient.entity.ts
├── value-objects/         # Immutable value objects
│   └── money.vo.ts
├── interfaces/            # Repository interfaces (contracts)
│   └── ingredient-repository.ts
├── errors/                # Domain-specific errors
│   └── ingredient-not-found.error.ts
└── constants.ts           # Business constants
```

**Example**:
```typescript
// Domain Entity
import { Entity } from "@libs/core";

export class Ingredient extends Entity {
  constructor(
    state: { name: string; description?: string },
    options: { datetimeProvider: DatetimeProvider }
  ) {
    super(state, options);
  }
  
  // Pure business logic methods
  rename(newName: string): void {
    // Business rules here
  }
}

// Domain Repository Contract (Abstract Class - preferred for NestJS DI)
export abstract class IngredientRepository {
  abstract save(ingredient: Ingredient): Promise<void>;
  abstract findById(id: string): Promise<Ingredient | null>;
  abstract delete(id: string): Promise<void>;
}

// ⚠️ Note: Use abstract classes instead of interfaces for DI tokens
// Why? TypeScript interfaces are erased at runtime, but NestJS needs
// runtime tokens for dependency injection. Abstract classes provide
// both compile-time contracts AND runtime tokens.

// ❌ Avoid interfaces for DI:
// export interface IngredientRepository { ... }  // No runtime token

// ✅ Prefer abstract classes for DI:
// export abstract class IngredientRepository { ... }  // Runtime token available

// 🔑 Important: Even though we use abstract classes, treat them as interfaces.
// Use 'implements' not 'extends' in concrete implementations.
```

**Dependencies**: `@libs/core` ONLY

#### 2. Application Layer (`@cooquoi/application`)

**Purpose**: Use cases and application orchestration

**Contains**:
- **Commands**: Write operations (`CreateIngredientCommand`)
- **Command Handlers**: Execute commands (CQRS pattern)
- **Application Services**: Orchestrate domain logic
- **DTOs**: Application-level data transfer objects

**Structure**:
```
libs/cooquoi-application/src/
├── commands/
│   ├── create-ingredient/
│   │   ├── create-ingredient.command.ts
│   │   └── create-ingredient.command-handler.ts
│   └── index.ts           # Export all command handlers
└── index.ts
```

**Example**:
```typescript
// Command
export class CreateIngredientCommand extends Command<void> {
  constructor(
    public readonly name: string,
    public readonly description?: string
  ) {
    super();
  }
}

// Command Handler
import { IngredientRepository } from "@cooquoi/domain";  // ✅ Uses abstract class
import { Ingredient } from "@cooquoi/domain";
import { DatetimeProvider } from "@libs/core";

@CommandHandler(CreateIngredientCommand)
export class CreateIngredientCommandHandler {
  constructor(
    private readonly ingredientRepository: IngredientRepository,  // ✅ Injected via abstract class
    private readonly datetimeProvider: DatetimeProvider
  ) {}
  
  async execute(command: CreateIngredientCommand): Promise<void> {
    const ingredient = new Ingredient(
      { name: command.name, description: command.description },
      { datetimeProvider: this.datetimeProvider }
    );
     ONLY
    await this.ingredientRepository.save(ingredient);  // ✅ Uses abstract method
  }
}
```

**Dependencies**: `@cooquoi/domain`, `@libs/core`

#### 3. Infrastructure Layer (`@cooquoi/infrastructure`)

**Purpose**: Technical implementations and external integrations

**Contains**:
- **Repository Implementations**: Concrete implementations of domain repository interfaces
- **Database Models**: ORM entities (TypeORM, Mongoose)
- **External Service Adapters**: API clients, message queues
- **Infrastructure Module**: NestJS module for DI configuration

**Structure**:
```
libs/cooquoi-infrastructure/src/
├── domain-impl/                    # Domain interface implementations
│   └── mongo-ingredient-repository.ts
├── typeorm/                        # TypeORM specific
│   ├── models/
│   │   └── ingredient.model.ts
│   └── seed-db.ts
├── mongoose/                       # Mongoose specific
│   └── schemas/
├── interfaces/                     # Infrastructure contracts
│   └── db-seedable.ts
└── cooquoi-infrastructure.module.ts
```

**Example**:
```typescript
// Repository Implementation
import { IngredientRepository } from "@cooquoi/domain";  // ✅ Implements abstract class
import { Ingredient } from "@cooquoi/domain";

@Injectable()
export class MongoIngredientRepository implements IngredientRepository {
  constructor(
    @InjectModel(IngredientModel.name) private model: Model<IngredientDocument>
  ) {}
  
  async save(ingredient: Ingredient): Promise<void> {
    // MongoDB specific implementation
    const doc = this.toDocument(ingredient);
    await this.model.create(doc);
  }
  
  async findById(id: string): Promise<Ingredient | null> {
    const doc = await this.model.findById(id);
    return doc ? this.toDomain(doc) : null;
  }
}

// Infrastructure Module (provides implementations)
@Module({})
export class CooquoiInfrastructureModule {
  static register(): DynamicModule {
    return {
      module: CooquoiInfrastructureModule,
      imports: [TypeOrmModule.forFeature(models, "CooquoiConnection")],
      providers: [
        // ✅ Simpler DI with abstract classes (no custom token needed)
        {
          provide: IngredientRepository,  // Abstract class as token
          useClass: MongoIngredientRepository  // Concrete implementation
        }
      ],
      exports: [IngredientRepository]
    };
  }
}
```Single gateway/facade for apps - the ONLY layer apps interact with

**Contains**:
- **Queries**: Read operations (CQRS query side)
- **Query Handlers**: Execute queries
- **Presentation Module**: Main module that bundles everything
- **Re-exports**: ALL domain entities, commands, and necessary infrastructure contracts that apps need

**Contains**:
- **Queries**: Read operations (CQRS query side)
- **Query Handlers**: Execute queries
- **Presentation Module**: Main module that bundles everything
- **Re-exported Infrastructure Contracts**: Selective infrastructure interfaces for apps

**Structure**:
```
libs/cooquoi-presentation/src/
├── queries/                        # CQRS query side
│   ├── get-ingredient/
│   │   ├── get-ingredient.query.ts
│   │   └── get-ingredient.query-handler.ts
│   └── index.ts
├── interfaces/                     # Presentation contracts
├── cooquoi.module.ts              # Main module
└── index.ts                       # Exports + selective infra re-exports
```

**Example**:
```typescript
// Presentation Module (Gateway)
import { commandHandlers } from "@cooquoi/application";
import { CooquoiInfrastructureModule } from "@cooquoi/infrastructure";
import { queryHandlers } from "./queries";

@Module({})
export class CooquoiModule {
  static register(): DynamicModule {
    return {
      module: CooquoiModule,
      imports: [CooquoiInfrastructureModule.register()],  // Wires infrastructure
   Comprehensive Re-exports (index.ts)
// Apps should ONLY import from this module

// Re-export Domain entities and value objects
export { Ingredient } from "@cooquoi/domain";
export type { IngredientRepository } from "@cooquoi/domain";  // Interface only

// Re-export Application layer commands
export { CreateIngredientCommand } from "@cooquoi/application";
export { DeleteIngredientCommand } from "@cooquoi/application";

// Re-export queries (native to presentation)
export * from "./queries";

// Re-export main module
export { CooquoiModule } from "./cooquoi.module";

// Re-export selective Infrastructure contracts apps need
export { DATA_SOURCES as TYPE_ORM_DATA_SOURCES } from "@cooquoi/infrastructure";
export { DbSeedable } from "@cooquoi/infrastructure";
export type { TypeOrmDataSource } from "@cooquoi/infrastructure";

// ❌ DON'T do full barrel exports or expose implementations
// export * from "@cooquoi/infrastructure";  // NEVER
// export { MongoIngredientRepository }xpose what apps need
export { DATA_SOURCES , DbSeedable } from "@cooquoi/presentation";  // ✅ ONLY import from presentation

@Module({
  imports: [
    ConfigModule.forRoot(),
    CqrsModule.forRoot(),
    CooquoiModule.register(),  // ✅ Gateway to all layers
  ],
  controllers: [IngredientsController],
  providers: [OnApplicationBootstrap]  // Uses DbSeedable through presentation
})
export class AppModule {}

// Controller - ALL imports from Presentation
import { 
  CreateIngredientCommand,  // ✅ Re-exported from application layer
  Ingredient                // ✅ Re-exported from domain
} from "@cooquoi/presentation";

// ❌ WRONG - Don't import from other layers
// import { CreateIngredientCommand } from "@cooquoi/application";  // NEVER
// import { Ingredient } from "@cooquoi/domain";                    // NEVER
// import { IngredientModel } from "@cooquoi/infrastructure";       // NEVER

@Controller('ingredients')
export class IngredientsController {
  constructor(private readonly commandBus: CommandBus) {}
  
  @Post()
  async create(@Body() dto: CreateIngredientDto) {
    await this.commandBus.execute(
      new CreateIngredientCommand(dto.name, dto.description)
    );
  }
}
```

**Dependencies**: `@cooquoi/presentation` ONLY (plus `@libs/utils-*` utilities)
  providers: [OnApplicationBootstrap]  // Uses DbSeedable through presentation
})
export class AppModule {}

// Controller
import { CreateIngredientCommand } from "@cooquoi/application";  // ✅ OK
import { Ingredient } from "@cooquoi/domain";                    // ✅ OK
// import { IngredientModel } from "@cooquoi/infrastructure";    // ❌ NEVER

@Controller('ingredients')
export class IngredientsController {
  constructor(private readonly commandBus: CommandBus) {}
  
  @Post()
  async create(@Body() dto: CreateIngredientDto) {
    await this.commandBus.execute(
      new CreateIngredientCommand(dto.name, dto.description)
    );
  }
}
```

**Dependencies**: `@cooquoi/presentation`, `@cooquoi/application`, `@cooquoi/domain`, `@libs/utils-*`

### Key Patterns

#### 1. Repository Pattern
- **Abstract Class in Domain**: `IngredientRepository` abstract class (preferred for NestJS DI)
- **Implementation in Infrastructure**: `MongoIngredientRepository` implements abstract class (not extends)
- **Injection in Application layer**: Command handlers receive abstract class via DI

#### 2. CQRS Pattern
- **Commands**: Write operations in Application layer
- **Queries**: Read operations in Presentation layer
- **Handlers**: Execute commands/queries
- **Separation**: Optimized read models can differ from write models

#### 3. Gateway/Facade Pattern
- **Presentation as Gateway**: Apps only interact with presentation layer
- **Selective Exposure**: Presentation re-exports only necessary infrastructure contracts
- **Encapsulation**: Internal infrastructure details hidden from apps

### NestJS Dependency Injection

This project uses NestJS's dependency injection system with **abstract classes as injection tokens** (preferred over interfaces).

#### Why Abstract Classes Over Interfaces?

TypeScript interfaces are erased at runtime, but NestJS needs runtime tokens for dependency injection. Abstract classes provide both compile-time type contracts AND runtime DI tokens without requiring custom string tokens.

#### Pattern

```typescript
// ✅ Preferred: Abstract class in domain
export abstract class IngredientRepository {
  abstract save(ingredient: Ingredient): Promise<void>;
  abstract findById(id: string): Promise<Ingredient | null>;
  abstract delete(id: string): Promise<void>;
}

// ✅ Implementation implements abstract class (NOT extends)
@Injectable()
export class MongoIngredientRepository implements IngredientRepository {
  constructor(
    @InjectModel(IngredientModel.name) private model: Model<IngredientDocument>
  ) {}
  
  async save(ingredient: Ingredient): Promise<void> { /* ... */ }
  async findById(id: string): Promise<Ingredient | null> { /* ... */ }
  async delete(id: string): Promise<void> { /* ... */ }
}

// ✅ Simple DI registration (abstract class is the token)
@Module({})
export class CooquoiInfrastructureModule {
  static register(): DynamicModule {
    return {
      module: CooquoiInfrastructureModule,
      providers: [
        {
          provide: IngredientRepository,  // No string token needed!
          useClass: MongoIngredientRepository
        }
      ],
      exports: [IngredientRepository]
    };
  }
}

// ✅ Injection in consumer (command handler, query handler, etc.)
@CommandHandler(CreateIngredientCommand)
export class CreateIngredientCommandHandler {
  constructor(
    private readonly ingredientRepository: IngredientRepository  // Abstract class injected
  ) {}
  
  async execute(command: CreateIngredientCommand): Promise<void> {
    await this.ingredientRepository.save(/* ... */);
  }
}

// ❌ Avoid: Interface requires custom string tokens
export interface IngredientRepository { /* ... */ }  // Erased at runtime!

providers: [
  {
    provide: 'IIngredientRepository',  // Need custom string token
    useClass: MongoIngredientRepository
  }
]

// And injection becomes cumbersome:
constructor(
  @Inject('IIngredientRepository') private repo: IngredientRepository
) {}
```

#### Key Guidelines

1. **Define abstract classes in Domain layer** for all repository and service contracts
2. **Implement abstract classes in Infrastructure layer** for concrete implementations
3. **Use abstract class directly as DI token** - no `@Inject()` decorator needed
4. **Implement abstract classes in Infrastructure layer** - use `implements`, NOT `extends`
3. **Use abstract class directly as DI token** - no `@Inject()` decorator needed
4. **Always add `@Injectable()` decorator** to implementations
5. **Treat abstract classes as interfaces** - they're contracts, not base classes to inherit from

```typescript
// Domain: Unit tests (no dependencies)
describe('Ingredient Entity', () => {
  it('should create ingredient', () => {
    const ingredient = new Ingredient({ name: 'Salt' }, { ... });
    expect(ingredient.name).toBe('Salt');
  });
});

// Application layer: Unit tests with mocked repositories
describe('CreateIngredientCommandHandler', () => {
  it('should save ingredient', async () => {
    // ✅ Mock abstract class
    const mockRepo = {
      save: jest.fn(),
      findById: jest.fn(),
      delete: jest.fn()
    } as IngredientRepository;
    
    const handler = new CreateIngredientCommandHandler(mockRepo, ...);
    await handler.execute(new CreateIngredientCommand('Salt'));
    expect(mockRepo.save).toHaveBeenCalled();
  });
});

// Infrastructure: Integration tests with real DB
describe('MongoIngredientRepository', () => {
  it('should persist to database', async () => {
    // Test with test database
  });
});
```

### Migration Guide (Current → Target)

**Issues to Fix**:

1. **Remove ALL direct layer imports from apps**:
   ```typescript
   // ❌ Current (apps/cooquoi-api-admin/src/app.module.ts)
   import { SeedDb } from "@cooquoi/infrastructure";
   
   // ✅ Target
   import { DbSeedable } from "@cooquoi/presentation";
   ```

2. **Remove domain and application layer imports from apps**:
   ```typescript
   // ❌ Current (apps/cooquoi-api-admin/src/controllers/ingredients.controller.ts)
   import { CreateIngredientCommand } from "@cooquoi/application";
   import { Ingredient } from "@cooquoi/domain";
   
   // ✅ Target
   import { CreateIngredientCommand, Ingredient } from "@cooquoi/presentation";
   ```

3. **Remove infrastructure models from apps**:
   ```typescript
   // ❌ Current
   import { IngredientModel } from "@cooquoi/infrastructure";
   
   // ✅ Target
   import { Ingredient } from "@cooquoi/presentation";  // Domain entity via presentation
   // or create a presentation-layer DTO if needed
   ```

4. **Update presentation layer to re-export everything apps need**:
   - Add to [cooquoi-presentation/src/index.ts](libs/cooquoi-presentation/src/index.ts):
     - Domain entities: `export { Ingredient } from "@cooquoi/domain";`
     - Commands: `export { CreateIngredientCommand } from "@cooquoi/application";`
     - Infrastructure contracts: `export { DbSeedable } from "@cooquoi/infrastructure";`
   - Document what's re-exported and why in comments

### Migration Guide (Current → Target)

**Issues to Fix**:

1. **Remove direct infrastructure imports from apps**:
   ```typescript
   // ❌ Current (apps/cooquoi-api-admin/src/app.module.ts)
   import { SeedDb } from "@cooquoi/infrastructure";
   
   // ✅ Target
   import { DbSeedable } from "@cooquoi/presentation";
   ```

2. **Use interfaces instead of concrete models in controllers**:
   ```typescript
   // ❌ Current
   import { IngredientModel } from "@cooquoi/infrastructure";
   
   // ✅ Target
   import { Ingredient } from "@cooquoi/domain";  // Use domain entity
   // or create a proper DTO in presentation layer
   ```

3. **Ensure presentation layer selectively re-exports**:
   - Review [cooquoi-presentation/src/index.ts](libs/cooquoi-presentation/src/index.ts)
   - Only export infrastructure contracts apps need
   - Document what's exposed and why

## 3. Code conventions

### NestJs 

#### Dependency injection


## 4. Testing