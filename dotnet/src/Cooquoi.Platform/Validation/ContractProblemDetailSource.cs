using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Raidvahkiin.Ddd;
using Raidvahkiin.Ddd.AspNetCore;
using Wolverine.Http.FluentValidation;

namespace Cooquoi.Platform.Validation;

/// <summary>
/// FluentValidation failures in the contract's shape: <c>common.validation</c> with one
/// <see cref="FieldError"/> per failure, field as a camelCase JSON path, code <c>validation.*</c>.
/// </summary>
public sealed class ContractProblemDetailSource<T> : IProblemDetailSource<T>
{
    public ProblemDetails Create(T message, IReadOnlyList<ValidationFailure> failures) =>
        ProblemWriter.ToProblemDetails(new Problem(
            StatusCodes.Status400BadRequest,
            CommonError.Validation.Code,
            CommonError.Validation.MessageTemplate,
            Errors: [.. failures.Select(ToFieldError)]));

    private static FieldError ToFieldError(ValidationFailure failure) =>
        new(JsonPath.FromPropertyPath(failure.PropertyName), ValidationCodes.Of(failure), ValidationCodes.ParamsOf(failure));
}
