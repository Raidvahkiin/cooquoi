using Cooquoi.AppHost.Resources;

var builder = DistributedApplication.CreateBuilder(args);

var webappClientSecret = builder.AddGeneratedSecret("webapp-oidc-client-secret");

var database = builder.AddCooquoiDatabase();
var keycloak = builder.AddCooquoiKeycloak(webappClientSecret);
var api = builder.AddCooquoiApi(database, keycloak);
builder.AddCooquoiWebapp(api, keycloak, webappClientSecret);

builder.Build().Run();
