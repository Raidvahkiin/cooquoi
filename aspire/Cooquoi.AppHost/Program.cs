using Cooquoi.AppHost.Resources;

var builder = DistributedApplication.CreateBuilder(args);

var webappClientSecret = builder.AddGeneratedSecret("webapp-oidc-client-secret");

var redisWebappPassword = builder.AddGeneratedSecret("redis-webapp-password");

var database = builder.AddCooquoiDatabase();
var keycloak = builder.AddCooquoiKeycloak(webappClientSecret);
var api = builder.AddCooquoiApi(database, keycloak);
var cache = builder.AddCooquoiCache(redisWebappPassword);
builder.AddCooquoiWebapp(api, keycloak, cache, redisWebappPassword, webappClientSecret);

builder.Build().Run();
