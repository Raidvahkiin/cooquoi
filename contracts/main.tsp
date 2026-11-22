import "@typespec/http";
import "@typespec/openapi";
import "./common/main.tsp";
import "./contexts/household/main.tsp";
import "./contexts/foodpedia/main.tsp";

using Http;
using OpenAPI;

/** Cooquoi API — source of truth for every client (ADR 0009). */
@service(#{ title: "Cooquoi API" })
@info(#{ version: "1.0.0" })
@server("https://api.cooquoi.com", "Production")
@useAuth(BearerAuth)
namespace Cooquoi;
