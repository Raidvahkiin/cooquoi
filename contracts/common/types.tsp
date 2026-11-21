namespace Cooquoi.Common;

/** Opaque identifier (UUID v7 server-side). */
@format("uuid")
scalar Id extends string;

/** Supported UI and content languages (ADR 0012). */
union Locale {
  en: "en",
  fr: "fr",
  vi: "vi",
}
