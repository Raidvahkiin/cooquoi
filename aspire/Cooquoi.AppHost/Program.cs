using Cooquoi.AppHost.Resources;

var builder = DistributedApplication.CreateBuilder(args);

var webappClientSecret = builder.AddGeneratedSecret("webapp-oidc-client-secret");

var sessionStoreWebappPassword = builder.AddGeneratedSecret("session-store-webapp-password");
var sessionStoreAdminPassword = builder.AddGeneratedSecret("session-store-admin-password");

var database = builder.AddCooquoiDatabase();
var keycloak = builder.AddCooquoiKeycloak(webappClientSecret);
var api = builder.AddCooquoiApi(database, keycloak);
var sessionStore = builder.AddCooquoiSessionStore(sessionStoreWebappPassword, sessionStoreAdminPassword);
builder.AddCooquoiWebapp(api, keycloak, sessionStore, sessionStoreWebappPassword, webappClientSecret);

builder.Build().Run();
