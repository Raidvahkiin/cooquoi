// Drift gate (ADR 0009 §6): fails when `oasdiff diff -f json` between the TypeSpec document (base) and the
// ASP.NET one (revision) reports anything but the accepted differences below. Reads the diff from the file
// given as argument, or from stdin. Used by `@cooquoi/contracts:drift` and by CI after oasdiff/oasdiff-action.
//
// Excluded by oasdiff itself (`-e description,examples,summary,tags,title`): documentation text only. Tags too:
// TypeSpec tags by context, Wolverine by endpoint class.
import { readFileSync } from 'node:fs';

/** Top-level keys of the oasdiff JSON diff (dotted for nested ones) accepted as drift, each with its reason. */
const accepted = {
  openAPI:
    'Microsoft.OpenApi 2.x always writes 3.1.1; the TypeSpec emitter only offers 3.1.0. Same spec, patch revision.',
  'components.schemas':
    'Schema names differ (TypeSpec namespaces vs C# DTO names). Endpoints compare resolved schemas, so any structural change still shows under paths.',
  'components.parameters':
    'TypeSpec puts shared query parameters in components; ASP.NET inlines them. Compared per endpoint like schemas.',
};

/** One line per change: `paths › /v1/x › GET › … › type: {"added":["string"],"deleted":["integer"]}`. */
function* leaves(node, path = []) {
  const isBranch =
    node &&
    typeof node === 'object' &&
    !Array.isArray(node) &&
    !('from' in node) &&
    !('added' in node);
  if (!isBranch) {
    yield `- ${path.join(' › ')}: ${JSON.stringify(node)}`;
    return;
  }
  for (const [key, child] of Object.entries(node)) {
    yield* leaves(child, key === 'modified' ? path : [...path, key]);
  }
}

const text = readFileSync(process.argv[2] ?? 0, 'utf8').trim();
if (text === '') {
  console.error('drift: no oasdiff output (did oasdiff fail?)');
  process.exit(1);
}

const diff = JSON.parse(text);
for (const key of Object.keys(accepted)) {
  const [parent, child] = key.split('.');
  if (child === undefined) {
    delete diff[parent];
  } else if (diff[parent]) {
    delete diff[parent][child];
    if (Object.keys(diff[parent]).length === 0) delete diff[parent];
  }
}

if (Object.keys(diff).length > 0) {
  console.error(
    'drift: the ASP.NET document differs from the contract (base: contracts/generated/openapi.yaml, revision: dotnet/generated/openapi.json).',
  );
  console.error(
    'Fix the contract or the ASP.NET OpenAPI metadata, or accept the difference with a reason in contracts/scripts/drift-gate.mjs.\n',
  );
  for (const line of leaves(diff)) console.error(line);
  process.exit(1);
}

console.log(`drift: none (accepted: ${Object.keys(accepted).join(', ')})`);
