import { ClassValue, clsx } from "clsx";
import { twMerge } from "tailwind-merge";

export function cn(...classes: ClassValue[]) {
	return twMerge(clsx(...classes));
}

export function mod(modifier: string, ...classes: ClassValue[]) {
	const prefix = modifier.endsWith(":") ? modifier : `${modifier}:`;
	const resolved = clsx(...classes).trim();

	if (!resolved) {
		return "";
	}

	const prefixedTokens = resolved
		.split(/\s+/)
		.map((token) => {
			const isInvalid = token.includes(":");

			if (isInvalid) {
				console.warn(
					`[UI utils][mod] Skipped invalid class token "${token}". Expected an unprefixed utility class.`,
				);
				return null;
			}

			return `${prefix}${token}`;
		})
		.filter((token): token is string => token !== null);

	return twMerge(prefixedTokens.join(" "));
}
