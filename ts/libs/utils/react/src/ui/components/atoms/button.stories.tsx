import type { Meta, StoryObj } from "@storybook/react";
import { Button } from "./button";

const meta = {
	component: Button,
	title: "Button",
	argTypes: {
		color: {
			control: "inline-radio",
			options: ["primary", "secondary", "tertiary"],
		},
		variant: {
			table: { disable: true },
		},
		disabled: {
			control: "boolean",
		},
	},
	args: {
		children: "Click me",
		color: "primary",
	},
} satisfies Meta<typeof Button>;
export default meta;

type Story = StoryObj<typeof Button>;

export const Contained: Story = {
	args: { variant: "contained", disabled: true },
};

export const Outlined: Story = {
	args: { variant: "outlined", disabled: false },
};

export const Text: Story = {
	args: { variant: "text", disabled: false },
};
