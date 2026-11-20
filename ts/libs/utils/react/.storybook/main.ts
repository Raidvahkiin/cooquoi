import { dirname } from "node:path";
import { fileURLToPath } from "node:url";

import type { StorybookConfig } from "@storybook/react-vite";

const config: StorybookConfig = {
	stories: ["../src/**/*.@(mdx|stories.@(js|jsx|ts|tsx))"],
	addons: [],
	framework: {
		name: getAbsolutePath("@storybook/react-vite"),
		options: {},
	},
	viteFinal: async (config) => {
		return config;
	},
};

function getAbsolutePath(value: string): string {
	return dirname(fileURLToPath(import.meta.resolve(`${value}/package.json`)));
}

export default config;
