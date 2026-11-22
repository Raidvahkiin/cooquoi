import "@typespec/http";

using Http;

namespace Cooquoi.Common;

// Aliases, not models: spread into operations, never emitted as schemas.

/** Version of the returned resource, a quoted number such as `"3"`; send it back as `If-Match`. */
alias ETagHeader = {
  @header("ETag") etag: string;
};

/** Version the client last read (its `ETag`); missing → 428, stale → 412. */
alias IfMatchHeader = {
  @header("If-Match") ifMatch: string;
};

/** Client-generated UUID that makes a POST safe to retry; required on every authenticated POST. */
alias IdempotencyKeyHeader = {
  @header("Idempotency-Key")
  @format("uuid")
  idempotencyKey: string;
};
