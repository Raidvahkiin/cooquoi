namespace Cooquoi.Architecture.Tests;

/// <summary>
/// Every code the backend can return is declared in the contract (<c>contracts/</c>), and the
/// contract declares no code the backend never returns: clients translate codes from the contract.
/// </summary>
public sealed class ErrorCatalogueTests
{
    [Fact]
    public void Problem_codes_match_the_contract() =>
        ShouldMatch(ErrorCatalogue.BackendProblemCodes(), ErrorCatalogue.ContractProblemCodes());

    [Fact]
    public void Validation_codes_match_the_contract() =>
        ShouldMatch(ErrorCatalogue.BackendValidationCodes(), ErrorCatalogue.ContractValidationCodes());

    private static void ShouldMatch(IReadOnlySet<string> backend, IReadOnlySet<string> contract)
    {
        backend.ShouldNotBeEmpty();
        contract.ShouldNotBeEmpty();
        backend.Except(contract).Order().ShouldBeEmpty("returned by the backend but missing from the contract");
        contract.Except(backend).Order().ShouldBeEmpty("declared in the contract but never returned by the backend");
    }
}
