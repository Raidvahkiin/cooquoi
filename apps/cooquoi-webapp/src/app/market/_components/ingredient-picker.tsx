import { useDebounce } from '@utils/react/hooks';
import { Input, TrashIcon } from '@utils/react/ui';
import { useEffect, useState } from 'react';
import { searchIngredients } from '@/actions/market.actions';

interface IngredientOption {
  id: string;
  name: string;
}

export function IngredientPicker({
  selectedIds,
  onChange,
}: {
  selectedIds: string[];
  onChange: (ids: string[]) => void;
}) {
  const [results, setResults] = useState<IngredientOption[]>([]);
  const [search, setSearch] = useState('');
  const debouncedSearch = useDebounce(search, 300);
  const [selectedNames, setSelectedNames] = useState<Map<string, string>>(
    new Map(),
  );

  useEffect(() => {
    const trimmed = debouncedSearch.trim();
    if (!trimmed) {
      setResults([]);
      return;
    }
    (async () => {
      try {
        const rows = await searchIngredients(trimmed);
        setResults(rows.filter((r) => !selectedIds.includes(r.id)));
      } catch {
        // silently fail
      }
    })();
  }, [debouncedSearch, selectedIds]);

  const nameFor = (id: string) => selectedNames.get(id) ?? id;

  return (
    <div className="flex flex-col gap-2">
      <span className="text-sm font-medium text-neutral-700">Ingredients</span>

      {selectedIds.length > 0 && (
        <div className="flex flex-wrap gap-1.5">
          {selectedIds.map((id) => (
            <span
              key={id}
              className="inline-flex items-center gap-1 rounded-full bg-primary/10 px-2.5 py-1 text-xs font-medium text-primary"
            >
              {nameFor(id)}
              <button
                type="button"
                onClick={() => onChange(selectedIds.filter((i) => i !== id))}
                className="ml-0.5 rounded-full p-0.5 hover:bg-primary/20 transition-colors"
                aria-label={`Remove ${nameFor(id)}`}
              >
                <TrashIcon className="size-3" />
              </button>
            </span>
          ))}
        </div>
      )}

      <div className="relative">
        <Input
          placeholder="Search ingredients…"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        {search && results.length > 0 && (
          <ul className="absolute z-10 mt-1 max-h-40 w-full overflow-auto rounded-md border border-border bg-surface shadow-md">
            {results.map((ing) => (
              <li key={ing.id}>
                <button
                  type="button"
                  className="w-full px-3 py-1.5 text-left text-sm hover:bg-neutral-100 transition-colors"
                  onClick={() => {
                    onChange([...selectedIds, ing.id]);
                    setSelectedNames((prev) =>
                      new Map(prev).set(ing.id, ing.name),
                    );
                    setSearch('');
                  }}
                >
                  {ing.name}
                </button>
              </li>
            ))}
          </ul>
        )}
        {search && results.length === 0 && (
          <p className="absolute z-10 mt-1 w-full rounded-md border border-border bg-surface px-3 py-2 text-xs text-neutral-400 shadow-md">
            No matching ingredients.
          </p>
        )}
      </div>

      {selectedIds.length === 0 && (
        <p className="text-xs text-neutral-400">
          No ingredients selected — optional.
        </p>
      )}
    </div>
  );
}
