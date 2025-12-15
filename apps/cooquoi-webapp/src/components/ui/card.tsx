import { FC, PropsWithChildren } from "react";

export const Card: FC<PropsWithChildren<{ className?: string }>> = ({
	children,
	className,
}) => {
	return (
		<div
			className={`bg-ui-surface shadow-md rounded-ui-lg p-6 ${className ?? ""}`}
		>
			{children}
		</div>
	);
};
