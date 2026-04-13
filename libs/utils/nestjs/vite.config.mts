/// <reference types='vitest' />
import { defineProject } from "vitest/config";

export default defineProject(() => ({
	root: __dirname,
	cacheDir: "node_modules/.vite/libs/core",
	resolve: {
		tsconfigPaths: true,
	},
	test: {
		name: "core",
		watch: false,
		globals: true,
		environment: "jsdom",
		include: ["{src,tests}/**/*.{test,spec}.{js,mjs,cjs,ts,mts,cts,jsx,tsx}"],
	},
}));
