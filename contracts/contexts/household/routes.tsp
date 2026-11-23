import "@typespec/http";
import "@typespec/openapi";
import "./models.tsp";

using Http;
using OpenAPI;
using Cooquoi.Common;

namespace Cooquoi.Household;

@tag("Household")
interface MeOperations {
  /**
   * Returns the signed-in user's profile and households. Read-only: creates nothing.
   * Errors: `common.not-found` (404) before the first `signIn`.
   */
  @get
  @route("/v1/me")
  @operationId("getMe")
  getMe(): Me | UnauthorizedProblem | NotFoundProblem;

  /**
   * Records a sign-in; call once per login. Creates the profile and a personal household on the
   * first call, later ones refresh name and email from the identity provider (ADR 0015).
   * Declared by hand: `ActionOperation` requires a body and an `id` path.
   */
  @post
  @route("/v1/me:sign-in")
  @operationId("signIn")
  signIn(...IdempotencyKeyHeader):
    | {
        @statusCode _: 204;
      }
    | BadRequestProblem
    | UnauthorizedProblem
    | ConflictProblem;
}
