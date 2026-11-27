# E02.S04 review fixes (after T05, before the story review): one commit `refactor(foodventory): … (E02.S04 review)`

These came from the user's review of T01–T03, relayed by the epic session.

- [x] 1. **`expires_on` is mapped twice.** (Partly done in T05 6e82742: the shadow `ExpiresOn` and its interceptor line are gone, and the index is hand-written SQL in the migration. Still to do: move the interceptor to `Persistence/Interceptors/`, check that the date has a single mapping, and add the round-trip tests.) In `Persistence/Configurations/IngredientStockConfiguration.cs` (~57-65), the owned `Expiry.Date` and the shadow `ExpiresOn` share one column, and `StockItemOwnerInterceptor` keeps them in step. That's fragile, and S05 changes expiries a lot. Map the expiry so the date has a single property: either flat item columns `expires_on` + `expiry_source` through a private property or value converter, or keep the owned type and create the `(household_id, expires_on)` index in the regenerated migration. Drop the `ExpiresOn` shadow property and its interceptor line. Keep the household and ingredient copies.
   - Move `StockItemOwnerInterceptor` from `Configurations/` to `Persistence/Interceptors/`.
   - Regenerate `InitialFoodventory` and keep the hand-written unaccent SQL.
   - Tests: the expiry is persisted and indexed, and a moved or changed expiry round-trips.
- [x] 2. **The Foodpedia adapter makes one query per ingredient per unit** (`Integration/FoodpediaFactsProvider.cs`) and works out the shelf life with a fixed probe date. Change the Foodpedia PublicApi (`IFoodpediaCatalog`, `CatalogIngredient`, `FoodpediaCatalog`):
   - P1 `CatalogIngredient` gains `int? ShelfLifeDays` and `bool IsActive`. Keep `Status` only if something else reads it.
   - P2 gets a batch form: the default factor of several units for several ingredients in one query, e.g. `GetDefaultFactorsAsync(ids, units)` → `ingredientId → unit → base amount`. Keep the single `NormalizeQuantityAsync` only if something still uses it.
   - `FoodpediaFactsProvider` then makes two queries in total. Drop `ShelfLifeProbe`, the per-unit loop and `Status == "active"`. Update the `IFoodpediaCatalog` doc to say camelCase codes (ADR 0021), not kebab-case.
   - Tests: Foodpedia PublicApi tests for the new fields and the batch, plus the adapter.
- [x] 3. **`HasMaxLength(20)` is repeated** on code columns: base unit, expiry kind, source, expiry source, and the pantry storage type in `PantryConfiguration`. Add one shared constant (e.g. `ColumnLengths.Code = 20` in `Cooquoi.Platform.Persistence`) or an EF convention for enum and unit code columns. Use it in the Foodventory and Foodpedia contexts, plus a test that every enum code and every unit code fits.

- [x] 4. Rename `StockError` to `IngredientStockError`, since an `<X>Error` is named after its aggregate (like `PantryError`). Update every reference and test.
- [x] 5. (Covered by fix 2: `IsActive` replaces `Status == "active"`.)
- [x] 6. `StockItemMovedDomainEvent.Unit` is a `string` while every other field is typed. Make it the `Unit` type, and convert to a code only where an integration event or a response needs one.
- [x] 7. In `IngredientStockAggregate.MoveItem` (`ToEnteredUnit`, ~:105-111), an amount that rounds to 0 at 3 decimals in the entered unit (0.4 g from an item entered in kg) ends in a confusing "0 is not a valid quantity". Reject it before or right after rounding with a clear error giving the amount and unit the user typed. Reuse `foodventory.invalidQuantity` with those params, or add a code to the contract and webapp messages only if needed. Add a domain test.
- [x] 8. Moving a whole item to the pantry it is already in, with no new expiry, changes nothing and raises no event (like renaming to the same name). Add a domain test.
- [x] 9. (Covered by fix 1: the interceptor moves to `Persistence/Interceptors/`.)
- [x] 10. `IngredientStockAggregate.BaseUnit = Unit.Gram` looks like a business default but exists only for EF. Use `= null!`, as `PantryAggregate.Name` does.
- [x] 11. (From the S03 review) In `Features/SetPantryStorage/SetPantryStorageEndpoint.cs` `LoadAsync`, use `using` directives instead of the qualified `Integration.CurrentHousehold` and `Persistence.FoodventoryDbContext`.
- [x] 12. (From the S03 review) `ListPantriesEndpoint` sorts by the raw name. Order by the `NameKey` shadow column (case- and accent-insensitive, like FV1), then by name.

Verify with:
```bash
dotnet build dotnet/Cooquoi.slnx -v q
dotnet test --solution dotnet/Cooquoi.slnx -v q --no-progress
pnpm nx run-many -t drift test
```

Model: sonnet

Status: done

Report:
Done 1-4, 6-8, 10-12 (5, 9 covered by 2 and 1). Fix 1: single `expires_on` mapping already held; interceptor moved to `Persistence/Interceptors/`, round-trip and index tests added. Model unchanged, so `InitialFoodventory` stays as is (no pending model changes).
Fix 2: P1 gets `IsActive` and `ShelfLifeDays`, P2 is batch `GetDefaultFactorsAsync` (universal-unit factors left out); single P2 and P3 (unused) and their records removed; adapter makes 2 queries.
Fix 3: `ColumnLengths.Code` in Platform, used in Foodventory and Foodpedia configurations, plus a fit test. Fix 7 reuses `foodventory.invalidQuantity` with the typed amount and unit params (message unchanged).
Verified: build, 273 solution tests green, format clean, `nx drift test` green.
