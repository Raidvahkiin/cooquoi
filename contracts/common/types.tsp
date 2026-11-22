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

/** Unit codes of the fixed unit catalogue (Foodpedia domain model); adding one is a contract change. */
union Unit {
  g: "g",
  kg: "kg",
  ml: "ml",
  l: "l",
  tsp: "tsp",
  tbsp: "tbsp",
  cup: "cup",
  piece: "piece",
  pack: "pack",
  can: "can",
  bottle: "bottle",
  slice: "slice",
  fillet: "fillet",
  clove: "clove",
  bunch: "bunch",
  pinch: "pinch",
}

/** Units an ingredient may be measured in by default. */
union BaseUnit {
  g: "g",
  ml: "ml",
  piece: "piece",
}
