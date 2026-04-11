import type { FC, InputHTMLAttributes } from "react";

export const Input: FC<InputHTMLAttributes<HTMLInputElement>> = ({
	className,
	...rest
}) => {
	return (
		<input
			className={`w-full rounded-md border border-border bg-surface px-3 py-2 text-sm text-text placeholder:text-text-muted focus:outline-none focus:ring-2 focus:ring-primary-ring ${
				className ?? ""
			}`}
			{...rest}
		/>
	);
};
