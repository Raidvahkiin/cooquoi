ALTER TABLE "offers" RENAME COLUMN "price" TO "price_currency";--> statement-breakpoint
ALTER TABLE "offers" ADD COLUMN "price_amount" numeric(12,2) NOT NULL;--> statement-breakpoint
ALTER TABLE "offers" DROP COLUMN "ingredients";--> statement-breakpoint
ALTER TABLE "offers" ALTER COLUMN "price_currency" SET DATA TYPE varchar(3) USING "price_currency"::varchar(3);