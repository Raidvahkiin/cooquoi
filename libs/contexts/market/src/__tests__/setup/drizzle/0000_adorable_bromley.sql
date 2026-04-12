CREATE TABLE "ingredient" (
	"id" uuid PRIMARY KEY DEFAULT gen_random_uuid() NOT NULL,
	"name" varchar(255) NOT NULL,
	"description" text,
	"created_at" date DEFAULT now() NOT NULL,
	"updated_at" date DEFAULT now() NOT NULL,
	CONSTRAINT "ingredient_name_unique" UNIQUE("name")
);
