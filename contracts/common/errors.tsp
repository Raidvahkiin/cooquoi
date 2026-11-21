import "@typespec/http";

using Http;

namespace Cooquoi.Common;

/** Stable error codes shared by every context (ADR 0014 §9). */
union CommonErrorCode {
  validation: "common.validation",
  notFound: "common.not-found",
  unauthorized: "common.unauthorized",
  forbidden: "common.forbidden",
  concurrencyConflict: "common.concurrency-conflict",
  versionMismatch: "common.version-mismatch",
  ifMatchRequired: "common.if-match-required",
  idempotencyKeyReused: "common.idempotency-key-reused",
  requestInProgress: "common.request-in-progress",
  unexpected: "common.unexpected",
}

/** One field error of a validation problem, translatable by code. */
model FieldError {
  /** JSON path of the field in the request, e.g. `names[1].primary`. */
  field: string;

  /** Stable code, e.g. `validation.required`, `validation.max-length`. */
  code: string;

  /** Values for the translated message, e.g. `{ "max": 100 }`. */
  params?: Record<unknown>;
}

/** RFC 9457 problem details with Cooquoi extensions (ADR 0014 §10). */
@error
model ProblemDetails {
  type: url;
  title: string;
  status: int32;
  detail?: string;
  instance?: string;

  /** Stable machine code clients translate (`<context>.<error>`). */
  code: string;

  /** Values for the translated message. */
  params?: Record<unknown>;

  /** Field errors, present on validation problems. */
  errors?: FieldError[];

  /** W3C trace id, to correlate with logs. */
  traceId?: string;
}

/** 401 — missing or invalid access token. */
@error
model UnauthorizedProblem {
  @statusCode _: 401;
  @header contentType: "application/problem+json";
  @body body: ProblemDetails;
}
