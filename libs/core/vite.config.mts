/// <reference types='vitest' />
import { defineProject } from "vitest/config";
import tsconfigPaths from "vite-tsconfig-paths";

export default defineProject(() => ({
	root: __dirname,
	cacheDir: "../../node_modules/.vite/libs/core",
	plugins: [tsconfigPaths()],
	test: {
		name: "@libs/core",
		watch: false,
		globals: true,
		environment: "jsdom",
		include: ["{src,tests}/**/*.{test,spec}.{js,mjs,cjs,ts,mts,cts,jsx,tsx}"],
	},
}));
