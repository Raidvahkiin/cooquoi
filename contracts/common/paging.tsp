import "@typespec/http";

using Http;

namespace Cooquoi.Common;

/** Query of a list operation: typed filters, free-text search, sort and offset paging (ADR 0009 §3). */
model ListQuery<TFilter extends {}> {
  ...TFilter;

  /** Free-text search. */
  @query q?: string;

  /** Sort fields among the ones the resource whitelists; a leading `-` sorts descending, e.g. `-expiresAt`. */
  @query(#{ explode: true }) sort?: string[];

  /** 1-based page number. */
  @query @minValue(1) page?: int32 = 1;

  /** Page size, at most 100 (server-enforced). */
  @query
  @minValue(1)
  @maxValue(100)
  pageSize?: int32 = 20;
}

/** One page of a list. */
model Page<T> {
  items: T[];
  page: int32;
  pageSize: int32;
  totalCount: int64;
}
