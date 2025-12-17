import { ApiProperty, ApiPropertyOptional } from "@nestjs/swagger";

export type AgGridSortDirection = "asc" | "desc";

export class AgGridSortModelItemDto {
	@ApiProperty({ type: String, example: "name" })
	colId!: string;

	@ApiProperty({ type: String, enum: ["asc", "desc"], example: "asc" })
	sort!: AgGridSortDirection;
}

export type AgGridFilterModel = Record<string, unknown>;

export class AgGridServerSideGetRowsRequestDto {
	@ApiProperty({ type: Number, example: 0 })
	startRow!: number;

	@ApiProperty({ type: Number, example: 100 })
	endRow!: number;

	@ApiPropertyOptional({ type: [AgGridSortModelItemDto] })
	sortModel?: AgGridSortModelItemDto[];

	@ApiPropertyOptional({
		description: "AG Grid filterModel as-is (minimal support server-side)",
		type: Object,
	})
	filterModel?: AgGridFilterModel;
}

export class AgGridServerSideGetRowsResponseDto<TRow> {
	@ApiProperty({ isArray: true })
	rows!: TRow[];

	@ApiProperty({
		description:
			"Total row count. Return -1 if unknown, otherwise the total number of matching rows.",
		example: 123,
		type: Number,
	})
	lastRow!: number;
}
