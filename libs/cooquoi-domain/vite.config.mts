/// <reference types='vitest' />
import { defineConfig } from "vite";
import tsconfigPaths from "vite-tsconfig-paths";

export default defineConfig(() => ({
	root: __dirname,
	cacheDir: "../../node_modules/.vite/libs/cooquoi",
	plugins: [tsconfigPaths()],
	// Uncomment this if you are using workers.
	// worker: {
	//  plugins: [],
	// },
	test: {
		name: "@libs/cooquoi-domain",
		watch: false,
		globals: true,
		environment: "jsdom",
		include: ["{src,tests}/**/*.{test,spec}.{js,mjs,cjs,ts,mts,cts,jsx,tsx}"],
		reporters: ["default"],
		coverage: {
			reportsDirectory: "./test-output/vitest/coverage",
			provider: "v8" as const,
		},
	},
}));
