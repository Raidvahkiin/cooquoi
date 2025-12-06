import { defineConfig } from "vitest/config";

export default defineConfig({
	test: {
		projects: ["libs/*/vite.config.mts", "apps/*/vite.config.mts"],
		reporters: ["default"],
		passWithNoTests: true,
		coverage: {
			provider: "v8",
		},
	},
});
