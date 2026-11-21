import "@typespec/http";
import "@typespec/openapi";
import "./common/main.tsp";
import "./contexts/household/main.tsp";

using Http;

/** Cooquoi API — source of truth for every client (ADR 0009). */
@service(#{ title: "Cooquoi API" })
@server("https://api.cooquoi.com", "Production")
@useAuth(BearerAuth)
namespace Cooquoi;
