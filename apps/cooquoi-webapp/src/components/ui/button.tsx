import type { ButtonHTMLAttributes, FC, PropsWithChildren } from "react";

type ButtonVariant = "primary" | "danger" | "ghost";

export const Button: FC<
	PropsWithChildren<
		ButtonHTMLAttributes<HTMLButtonElement> & { variant?: ButtonVariant }
	>
> = ({
	children,
	className,
	variant = "primary",
	type = "button",
	...rest
}) => {
	const base =
		"inline-flex items-center justify-center rounded-ui px-ui-3 py-ui-2 text-sm font-medium transition-colors focus:outline-none focus:ring-2 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed";

	const byVariant: Record<ButtonVariant, string> = {
		primary:
			"bg-ui-primary text-white hover:bg-ui-primaryHover focus:ring-ui-primaryRing",
		danger:
			"bg-ui-danger text-white hover:bg-ui-dangerHover focus:ring-ui-dangerRing",
		ghost:
			"bg-transparent text-ui-text hover:bg-ui-surfaceMuted focus:ring-ui-border",
	};

	return (
		<button
			type={type}
			className={`${base} ${byVariant[variant]} ${className ?? ""}`}
			{...rest}
		>
			{children}
		</button>
	);
};
