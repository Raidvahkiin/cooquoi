import "@typespec/http";
import "./types.tsp";
import "./errors.tsp";
import "./headers.tsp";
import "./paging.tsp";

using Http;

namespace Cooquoi.Common;

// Operation kinds every context reuses (ADR 0009 §2). Suffixed `Operation`: `Read` and
// `Create` would clash with TypeSpec visibility templates. An instance adds the route:
// `@route("/v1/ingredients/{ingredientId}:rename") op renameIngredient is ReplaceActionOperation<RenameIngredient, IngredientPath>;`
// `TPath` names the path parameter as the backend route does (default `{id}`), e.g. `alias IngredientPath = { @path ingredientId: Id };`.

/** Lists resources: typed filters, search, sort and paging. */
@get
op ListOperation<TFilter extends {}, TItem>(...ListQuery<TFilter>):
  | Page<TItem>
  | BadRequestProblem
  | UnauthorizedProblem;

/** Reads one resource by id; `ETag` carries its version. */
@get
op ReadOperation<T, TPath extends {} = { @path id: Id }>(...TPath):
  | {
      @body body: T;
      ...ETagHeader;
    }
  | UnauthorizedProblem
  | NotFoundProblem;

/** Creates a resource; `Location` points to it. */
@post
op CreateOperation<TBody extends {}, TCreated>(
  ...IdempotencyKeyHeader,
  @body body: TBody,
):
  | {
      @statusCode _: 201;
      @header("Location") location: string;
      @body body: TCreated;
    }
  | BadRequestProblem
  | UnauthorizedProblem
  | ForbiddenProblem
  | ConflictProblem
  | UnprocessableProblem;

/** Named action on a resource (`POST /…/{id}:verb`); `ETag` carries the new version. */
@post
op ActionOperation<TBody extends {}, TPath extends {} = { @path id: Id }>(
  ...TPath,
  ...IdempotencyKeyHeader,
  @body body: TBody,
):
  | {
      @statusCode _: 204;
      ...ETagHeader;
    }
  | BadRequestProblem
  | UnauthorizedProblem
  | ForbiddenProblem
  | NotFoundProblem
  | ConflictProblem
  | UnprocessableProblem;

/** Named action that returns a body (`POST /…/{id}:verb`): `200` with the result, no `If-Match`. */
@post
op ActionWithResultOperation<TBody extends {}, TResult, TPath extends {} = {}>(
  ...TPath,
  ...IdempotencyKeyHeader,
  @body body: TBody,
):
  | {
      @statusCode _: 200;
      @body result: TResult;
    }
  | BadRequestProblem
  | UnauthorizedProblem
  | ForbiddenProblem
  | NotFoundProblem
  | ConflictProblem
  | UnprocessableProblem;

/** Action that replaces state from what the client read: requires `If-Match`. */
@post
op ReplaceActionOperation<TBody extends {}, TPath extends {} = { @path id: Id }>(
  ...TPath,
  ...IdempotencyKeyHeader,
  ...IfMatchHeader,
  @body body: TBody,
):
  | {
      @statusCode _: 204;
      ...ETagHeader;
    }
  | BadRequestProblem
  | UnauthorizedProblem
  | ForbiddenProblem
  | NotFoundProblem
  | ConflictProblem
  | PreconditionFailedProblem
  | UnprocessableProblem
  | PreconditionRequiredProblem;

/** Reads a small collection that is not paged: no filters, sort or paging. */
@get
op CollectionReadOperation<T>():
  | {
      @body body: T;
    }
  | UnauthorizedProblem;

/** Deletes a resource; requires `If-Match`. */
@delete
op DeleteOperation<TPath extends {} = { @path id: Id }>(
  ...TPath,
  ...IfMatchHeader,
):
  | {
      @statusCode _: 204;
    }
  | UnauthorizedProblem
  | ForbiddenProblem
  | NotFoundProblem
  | ConflictProblem
  | PreconditionFailedProblem
  | UnprocessableProblem
  | PreconditionRequiredProblem;
