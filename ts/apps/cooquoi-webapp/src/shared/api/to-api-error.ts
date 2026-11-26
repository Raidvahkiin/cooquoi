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
  'common.invalidSort': true,
  'common.notFound': true,
  'common.unauthorized': true,
  'common.forbidden': true,
  'common.concurrencyConflict': true,
  'common.versionMismatch': true,
  'common.ifMatchRequired': true,
  'common.idempotencyKeyRequired': true,
  'common.idempotencyKeyReused': true,
  'common.requestInProgress': true,
  'common.requestAlreadyProcessed': true,
  'common.unexpected': true,
  'foodpedia.codeTaken': true,
  'foodpedia.invalidCode': true,
  'foodpedia.invalidName': true,
  'foodpedia.englishNameRequired': true,
  'foodpedia.duplicateLocale': true,
  'foodpedia.nameMissing': true,
  'foodpedia.nameTaken': true,
  'foodpedia.invalidBaseUnit': true,
  'foodpedia.invalidConversionAmount': true,
  'foodpedia.conversionNotNeeded': true,
  'foodpedia.invalidShelfLife': true,
  'foodpedia.categoryTagRequired': true,
  'foodpedia.ingredientRetired': true,
  'foodpedia.invalidTagCode': true,
  'foodpedia.invalidTagLabel': true,
  'foodpedia.englishLabelRequired': true,
  'foodpedia.unknownTags': true,
  'validation.required': true,
  'validation.maxLength': true,
  'validation.minLength': true,
  'validation.min': true,
  'validation.max': true,
  'validation.range': true,
  'validation.pattern': true,
  'validation.invalid': true,
  'validation.oneOf': true,
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
