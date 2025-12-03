/// <reference types='vitest' />
import { resolve } from "node:path";
import { defineConfig } from "vite";

export default defineConfig(() => ({
	root: __dirname,
	cacheDir: "../../node_modules/.vite/libs/core",
	plugins: [],
	resolve: {
		alias: {
			src: resolve(__dirname, "./src"),
		},
	},
	// Uncomment this if you are using workers.
	// worker: {
	//  plugins: [],
	// },
	test: {
		name: "@libs/core",
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
