import z from "zod";

const envNumber = (
	envKey: string,
	fallback: number,
	schemaOrFn: z.ZodNumber | ((schema: z.ZodNumber) => z.ZodNumber) = z.number(),
) => {
	const schema =
		typeof schemaOrFn === "function" ? schemaOrFn(z.number()) : schemaOrFn;
	return z.preprocess((val) => {
		if (val !== undefined) {
			return typeof val === "string" ? Number.parseInt(val, 10) : val;
		}
		const envVal = process.env[envKey];
		return envVal ? Number.parseInt(envVal, 10) : fallback;
	}, schema);
};

const envString = (
	envKey: string,
	fallback: string,
	schemaOrFn: z.ZodString | ((schema: z.ZodString) => z.ZodString) = z.string(),
) => {
	const schema =
		typeof schemaOrFn === "function" ? schemaOrFn(z.string()) : schemaOrFn;
	return z.preprocess((val) => {
		if (val !== undefined) return val;
		return process.env[envKey] ?? fallback;
	}, schema);
};

const envBoolean = (
	envKey: string,
	fallback: boolean,
	schemaOrFn:
		| z.ZodBoolean
		| ((schema: z.ZodBoolean) => z.ZodBoolean) = z.boolean(),
) => {
	const schema =
		typeof schemaOrFn === "function" ? schemaOrFn(z.boolean()) : schemaOrFn;
	return z.preprocess((val) => {
		if (val !== undefined) {
			if (typeof val === "string") {
				return val.toLowerCase() === "true";
			}
			return val;
		}
		const envVal = process.env[envKey];
		if (envVal !== undefined) {
			return envVal.toLowerCase() === "true";
		}
		return fallback;
	}, schema);
};

const serverConfigSchema = z.object({
	port: envNumber("PORT", 3000, (schema) => schema.min(1).max(65535)),
	host: envString("HOST", "localhost", (schema) => schema.min(1)),
});

const postgresConfigSchema = z.object({
	host: envString("DB_POSTGRES_HOST", "localhost", (schema) => schema.min(1)),
	port: envNumber("DB_POSTGRES_PORT", 5432, (schema) =>
		schema.int().positive(),
	),
	database: envString("DB_POSTGRES_DB", "cooquoi", (schema) => schema.min(1)),
	username: envString("DB_POSTGRES_USERNAME", "cooquoiuser", (schema) =>
		schema.min(1),
	),
	password: envString("DB_POSTGRES_PASSWORD", "cooquoipassword", (schema) =>
		schema.min(8),
	),
	synchronize: envBoolean("DB_POSTGRES_SYNCHRONIZE", false),
	logging: envBoolean("DB_POSTGRES_LOGGING", false),
});

export const appConfigSchema = z.object({
	server: z.preprocess(
		(val) => serverConfigSchema.parse(val ?? {}),
		serverConfigSchema,
	),

	database: z.preprocess(
		(val) =>
			z
				.object({
					postgres: postgresConfigSchema,
				})
				.parse(
					val ?? {
						postgres: {},
					},
				),
		z.object({
			postgres: postgresConfigSchema,
		}),
	),
});

export type AppConfig = z.infer<typeof appConfigSchema>;
