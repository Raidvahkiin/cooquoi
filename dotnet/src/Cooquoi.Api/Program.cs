using Cooquoi.Context.Household;
using Cooquoi.Platform.Authentication;
using Raidvahkiin.Ddd.AspNetCore;
using Wolverine;
using Wolverine.Http;

const string DatabaseConnection = "cooquoi";

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddCooquoiAuthentication();
builder.AddHouseholdContext(DatabaseConnection);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddWolverineHttp();
builder.Host.UseWolverine(options => options.Discovery.IncludeAssembly(HouseholdModule.Assembly));

var app = builder.Build();
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    await app.MigrateHouseholdDatabaseAsync();
}

app.MapDefaultEndpoints();
app.MapWolverineEndpoints();
await app.RunAsync();
