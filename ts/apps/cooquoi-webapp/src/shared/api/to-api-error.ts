import type {
  CommonCommonErrorCode,
  CommonFieldError,
  CommonProblemDetails,
  CommonValidationCode,
  FoodpediaFoodpediaErrorCode,
} from '@cooquoi/api-client';

type ErrorParams = Record<string, unknown>;

/** A problem the UI can show: translated by `code`, with ICU `params`. */
export type ApiError = {
  code: string;
  params?: ErrorParams;
  fieldErrors?: Record<string, { code: string; params?: ErrorParams }[]>;
};

export const UNEXPECTED_ERROR_CODE =
  'common.unexpected' satisfies CommonCommonErrorCode;

type ErrorCode =
  | CommonCommonErrorCode
  | FoodpediaFoodpediaErrorCode
  | CommonValidationCode;

// A Record keyed by the contract unions: adding a code to the contract fails typecheck here.
const KNOWN_CODES: Record<ErrorCode, true> = {
  'common.validation': true,
  'common.invalid-sort': true,
  'common.not-found': true,
  'common.unauthorized': true,
  'common.forbidden': true,
  'common.concurrency-conflict': true,
  'common.version-mismatch': true,
  'common.if-match-required': true,
  'common.idempotency-key-required': true,
  'common.idempotency-key-reused': true,
  'common.request-in-progress': true,
  'common.request-already-processed': true,
  'common.unexpected': true,
  'foodpedia.code-taken': true,
  'foodpedia.invalid-code': true,
  'foodpedia.invalid-name': true,
  'foodpedia.english-name-required': true,
  'foodpedia.duplicate-locale': true,
  'foodpedia.name-missing': true,
  'foodpedia.name-taken': true,
  'foodpedia.invalid-base-unit': true,
  'foodpedia.invalid-conversion-amount': true,
  'foodpedia.conversion-not-needed': true,
  'foodpedia.invalid-shelf-life': true,
  'foodpedia.category-tag-required': true,
  'foodpedia.ingredient-retired': true,
  'foodpedia.invalid-tag-code': true,
  'foodpedia.invalid-tag-label': true,
  'foodpedia.english-label-required': true,
  'foodpedia.unknown-tags': true,
  'validation.required': true,
  'validation.max-length': true,
  'validation.min-length': true,
  'validation.min': true,
  'validation.max': true,
  'validation.range': true,
  'validation.pattern': true,
  'validation.invalid': true,
  'validation.one-of': true,
};

function knownCodeOrUnexpected(code: unknown): string {
  return typeof code === 'string' && Object.hasOwn(KNOWN_CODES, code)
    ? code
    : UNEXPECTED_ERROR_CODE;
}

function toFieldErrors(
  errors: CommonFieldError[] | undefined,
): ApiError['fieldErrors'] {
  if (!Array.isArray(errors) || errors.length === 0) return undefined;
  const fieldErrors: NonNullable<ApiError['fieldErrors']> = {};
  for (const { field, code, params } of errors) {
    const list = fieldErrors[field] ?? [];
    list.push({
      code: knownCodeOrUnexpected(code),
      ...(params && { params }),
    });
    fieldErrors[field] = list;
  }
  return fieldErrors;
}

/**
 * Maps a Hey API `error` (the problem body) or a thrown failure to an `ApiError`.
 * Unknown or missing codes, and network failures, become `common.unexpected`.
 */
export function toApiError(error: unknown): ApiError {
  if (typeof error !== 'object' || error === null) {
    return { code: UNEXPECTED_ERROR_CODE };
  }
  const problem = error as Partial<CommonProblemDetails>;
  const fieldErrors = toFieldErrors(problem.errors);
  return {
    code: knownCodeOrUnexpected(problem.code),
    ...(problem.params && { params: problem.params }),
    ...(fieldErrors && { fieldErrors }),
  };
}
