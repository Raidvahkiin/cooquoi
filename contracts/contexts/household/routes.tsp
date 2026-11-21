import "@typespec/http";
import "@typespec/openapi";
import "./models.tsp";

using Http;
using OpenAPI;
using Cooquoi.Common;

namespace Cooquoi.Household;

@tag("Household")
@route("/v1/me")
interface MeOperations {
  /**
   * Returns the signed-in user's profile and households.
   * On first sign-in, creates the profile and a personal household (ADR 0015).
   */
  @get
  @operationId("getMe")
  getMe(): Me | UnauthorizedProblem;
}
