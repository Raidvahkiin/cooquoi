import type { FilterQuery } from "@cooquoi/infrastructure";

export function escapeRegexLiteral(value: string): string {
	return value.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
}

export function buildFuzzySubsequencePattern(
	value: string,
	options?: {
		maxChars?: number;
	},
): string {
	// Turns "tmaTo" into "t.*m.*a.*T.*o" (escaped) for tolerant matching.
	const trimmed = value.trim();
	if (trimmed.length === 0) return "";

	// Guard against overly large/expensive patterns.
	const maxChars = options?.maxChars ?? 64;
	const slice = trimmed.slice(0, maxChars);

	const parts: string[] = [];
	for (const ch of slice) {
		if (ch === " ") continue;
		parts.push(escapeRegexLiteral(ch));
	}

	// If the input was only spaces, fall back to empty.
	if (parts.length === 0) return "";
	return parts.join(".*");
}

export function buildTextFilterPattern(
	type: string | undefined,
	rawValue: string,
): string | null {
	// Generic “grid text filter” mapping (client decides which type strings to send).
	if (type === "contains") return escapeRegexLiteral(rawValue);
	if (type === "fuzzy") return buildFuzzySubsequencePattern(rawValue);
	return null;
}

export function buildCaseInsensitiveRegexClause<TModel>(
	field: string,
	pattern: string,
): FilterQuery<TModel> {
	return {
		[field]: { $regex: pattern, $options: "i" },
	} as FilterQuery<TModel>;
}

export function buildTextFilterClause<TModel>(
	field: string,
	type: string | undefined,
	filter: unknown,
): FilterQuery<TModel> | null {
	if (typeof filter !== "string") return null;

	const rawValue = filter.trim();
	if (rawValue.length === 0) return null;

	const pattern = buildTextFilterPattern(type, rawValue);
	if (!pattern) return null;

	return buildCaseInsensitiveRegexClause<TModel>(field, pattern);
}

export function buildSetFilterClause<TModel>(
	field: string,
	values: unknown,
): FilterQuery<TModel> | null {
	const list = Array.isArray(values)
		? values.filter((v) => typeof v === "string")
		: [];
	if (list.length === 0) return null;

	return {
		[field]: { $in: list },
	} as FilterQuery<TModel>;
}
