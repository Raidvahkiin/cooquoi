import { FC, PropsWithChildren } from "react";

export const Card: FC<PropsWithChildren<{ className?: string }>> = ({
	children,
	className,
}) => {
	return (
		<div className={`bg-surface shadow-md rounded-lg p-6 ${className ?? ""}`}>
			{children}
		</div>
	);
};
