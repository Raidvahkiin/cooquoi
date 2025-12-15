import type { FC, InputHTMLAttributes } from "react";

export const Input: FC<InputHTMLAttributes<HTMLInputElement>> = ({
	className,
	...rest
}) => {
	return (
		<input
			className={`w-full rounded-ui border border-ui-border bg-ui-surface px-ui-3 py-ui-2 text-sm text-ui-text placeholder:text-ui-textMuted focus:outline-none focus:ring-2 focus:ring-ui-primaryRing ${
				className ?? ""
			}`}
			{...rest}
		/>
	);
};
