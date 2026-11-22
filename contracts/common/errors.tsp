import "@typespec/http";

using Http;

namespace Cooquoi.Common;

/** Stable error codes shared by every context (ADR 0014 §9). */
union CommonErrorCode {
  validation: "common.validation",
  invalidSort: "common.invalid-sort",
  notFound: "common.not-found",
  unauthorized: "common.unauthorized",
  forbidden: "common.forbidden",
  concurrencyConflict: "common.concurrency-conflict",
  versionMismatch: "common.version-mismatch",
  ifMatchRequired: "common.if-match-required",
  idempotencyKeyRequired: "common.idempotency-key-required",
  idempotencyKeyReused: "common.idempotency-key-reused",
  requestInProgress: "common.request-in-progress",
  requestAlreadyProcessed: "common.request-already-processed",
  unexpected: "common.unexpected",
}

/** Stable codes of field errors (`errors[].code`), shared by every validator. */
union ValidationCode {
  required: "validation.required",
  maxLength: "validation.max-length",
  minLength: "validation.min-length",
  min: "validation.min",
  max: "validation.max",
  range: "validation.range",
  pattern: "validation.pattern",
  invalid: "validation.invalid",
  oneOf: "validation.one-of",
}

/** One field error of a validation problem, translatable by code. */
model FieldError {
  /** JSON path of the field in the request, e.g. `names[1].primary`. */
  field: string;

  /** Stable code clients translate. */
  code: ValidationCode;

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

/** A problem response with its status code, served as `application/problem+json`. */
@error
model Problem<TStatus extends int32> {
  @statusCode _: TStatus;
  @header contentType: "application/problem+json";
  @body body: ProblemDetails;
}

/** 400 — invalid request: field errors, bad sort, missing `Idempotency-Key`. */
model BadRequestProblem is Problem<400>;

/** 401 — missing or invalid access token. */
model UnauthorizedProblem is Problem<401>;

/** 403 — the caller may not do this. */
model ForbiddenProblem is Problem<403>;

/** 404 — the resource does not exist or is not visible to the caller. */
model NotFoundProblem is Problem<404>;

/** 409 — conflicting state: duplicate, lost concurrency race, request still running or already processed. */
model ConflictProblem is Problem<409>;

/** 412 — `If-Match` does not match the current version. */
model PreconditionFailedProblem is Problem<412>;

/** 422 — a business rule rejects the request, or the `Idempotency-Key` was reused. */
model UnprocessableProblem is Problem<422>;

/** 428 — `If-Match` is required. */
model PreconditionRequiredProblem is Problem<428>;
