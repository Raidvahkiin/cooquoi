// Compact tree reporter: describe blocks indented, no file paths
const green = (s) => `\x1b[32m${s}\x1b[0m`;
const red = (s) => `\x1b[31m${s}\x1b[0m`;
const yellow = (s) => `\x1b[33m${s}\x1b[0m`;
const dim = (s) => `\x1b[2m${s}\x1b[0m`;
const bold = (s) => `\x1b[1m${s}\x1b[0m`;
const indent = (depth) => '  '.repeat(depth);

export default class CompactReporter {
  #results = new Map();

  onTestCaseResult(testCase) {
    const diag = testCase.diagnostic();
    this.#results.set(testCase, {
      state: testCase.result().state,
      duration: diag != null ? diag.duration : null,
    });
  }

  #printCollection(collection, depth) {
    for (const item of collection) {
      if (item.type === 'suite') {
        process.stdout.write(`${indent(depth) + bold(item.name)}\n`);
        this.#printCollection(item.children, depth + 1);
      } else {
        const r = this.#results.get(item);
        if (!r || r.state === 'pending') continue;
        const icon =
          r.state === 'passed' ? green('\u2713') :
          r.state === 'skipped' ? yellow('\u2193') :
          red('\u2717');
        const ms = r.duration != null ? ` ${dim(`${Math.round(r.duration)}ms`)}` : '';
        process.stdout.write(`${indent(depth) + icon} ${item.name}${ms}\n`);
      }
    }
  }

  onTestRunEnd(testModules, unhandledErrors) {
    for (const mod of testModules) {
      this.#printCollection(mod.children, 0);
    }

    let passed = 0;
    let failed = 0;
    let skipped = 0;
    for (const { state } of this.#results.values()) {
      if (state === 'passed') passed++;
      else if (state === 'failed') failed++;
      else if (state === 'skipped') skipped++;
    }
    const total = passed + failed + skipped;
    const parts = [
      passed && green(`${passed} passed`),
      failed && red(`${failed} failed`),
      skipped && dim(`${skipped} skipped`),
    ].filter(Boolean);
    console.log(`\n${bold('Tests')}  ${parts.join(', ')} (${total})`);
    for (const err of unhandledErrors) console.error(err);
  }
}
