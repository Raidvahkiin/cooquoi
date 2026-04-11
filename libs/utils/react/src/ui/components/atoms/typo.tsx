import { cva } from "class-variance-authority";
import { FC } from "react";
import { cn } from "../../utlis/class-name.util";

export type TypoProps = {
	type: "title" | "subtitle" | "body";
	size: "sm" | "md" | "lg";
	className?: string;
	text: string;
};

const TypoStyle = cva("font-sans", {
	variants: {
		type: {
			title: "font-bold",
			subtitle: "font-medium",
			body: "text-sm",
		},
		size: {
			sm: "",
			md: "",
			lg: "",
		},
	},
	compoundVariants: [
		{
			type: "title",
			size: "sm",
			className: `
            text-lg
        `,
		},
	],
});

export const Typo: FC<TypoProps> = ({ className, text, type, size }) => {
	return (
		<span className={cn(TypoStyle({ type, size }), className)}>{text}</span>
	);
};
