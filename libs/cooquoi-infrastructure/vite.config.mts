/// <reference types='vitest' />
import { defineProject } from "vitest/config";
import tsconfigPaths from "vite-tsconfig-paths";

export default defineProject(() => ({
	root: __dirname,
	cacheDir: "../../node_modules/.vite/libs/cooquoi-infrastructure",
	plugins: [tsconfigPaths()],
	test: {
		name: "@cooquoi/infrastructure",
		watch: false,
		globals: true,
		environment: "jsdom",
		passWithNoTests: true,
		include: ["{src,tests}/**/*.{test,spec}.{js,mjs,cjs,ts,mts,cts,jsx,tsx}"],
	},
}));
