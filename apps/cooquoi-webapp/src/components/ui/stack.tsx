import type { HTMLAttributes, ReactNode } from "react";

type Direction = "row" | "col";
type Align = "start" | "center" | "end" | "stretch" | "baseline";
type Justify = "start" | "center" | "end" | "between" | "around" | "evenly";
type Gap = "none" | "xs" | "sm" | "md" | "lg" | "xl";

export type StackProps = HTMLAttributes<HTMLDivElement> & {
	children?: ReactNode;
	direction?: Direction;
	align?: Align;
	justify?: Justify;
	wrap?: boolean;
	gap?: Gap;
};

const cx = (...classes: Array<string | undefined | false | null>) =>
	classes.filter(Boolean).join(" ");

const directionClass: Record<Direction, string> = {
	row: "flex-row",
	col: "flex-col",
};

const alignClass: Record<Align, string> = {
	start: "items-start",
	center: "items-center",
	end: "items-end",
	stretch: "items-stretch",
	baseline: "items-baseline",
};

const justifyClass: Record<Justify, string> = {
	start: "justify-start",
	center: "justify-center",
	end: "justify-end",
	between: "justify-between",
	around: "justify-around",
	evenly: "justify-evenly",
};

const gapClass: Record<Gap, string> = {
	none: "gap-0",
	xs: "gap-1",
	sm: "gap-2",
	md: "gap-3",
	lg: "gap-4",
	xl: "gap-6",
};

export function Stack({
	children,
	className,
	direction = "col",
	align,
	justify,
	wrap = false,
	gap = "md",
	...rest
}: StackProps) {
	return (
		<div
			className={cx(
				"flex",
				directionClass[direction],
				gapClass[gap],
				align ? alignClass[align] : undefined,
				justify ? justifyClass[justify] : undefined,
				wrap ? "flex-wrap" : undefined,
				className,
			)}
			{...rest}
		>
			{children}
		</div>
	);
}
