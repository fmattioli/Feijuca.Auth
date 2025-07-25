using Coderaw.Settings.Extensions.Handlers;
using Coderaw.Settings.Transformers;
using Feijuca.Auth.Common.Models;
using Feijuca.Auth.Extensions;
using Feijuca.Auth.Infra.CrossCutting.Extensions;
using Feijuca.Auth.Infra.CrossCutting.Middlewares;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
var enviroment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

builder.Configuration
    .AddJsonFile("appsettings.json", false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{enviroment}.json", true, reloadOnChange: true)
    .AddEnvironmentVariables();

var applicationSettings = builder.Configuration.GetApplicationSettings(builder.Environment);

builder.Services
    .AddExceptionHandler<GlobalExceptionHandler>()
    .AddProblemDetails()
    .AddMediator()
    .AddRepositories()
    .AddValidators()
    .AddServices()
    .AddOpenApi("v1", options => { options.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); })
    .AddMongo(applicationSettings)
    .AddApiAuthentication(out KeycloakSettings KeycloakSettings)
    .AddEndpointsApiExplorer()
    .AddSwagger(KeycloakSettings)
    .AddHttpClients()
    .ConfigureValidationErrorResponses()
    .AddCors(options =>
    {
        options.AddPolicy("AllowAllOrigins", policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
    })
    .AddControllers();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseCors("AllowAllOrigins")
   .UseExceptionHandler()
   .UseSwagger()
   .UseSwaggerUI(c =>
   {
       c.SwaggerEndpoint("/swagger/v1/swagger.json", "Feijuca.Auth.Api");
       c.OAuthUseBasicAuthenticationWithAccessCodeGrant();
   });

if (KeycloakSettings?.Realms?.Any() ?? false)
{
    app.UseAuthorization()
       .UseTenantMiddleware();
}

app.UseHttpsRedirection()
   .UseHttpsRedirection()
   .UseMiddleware<ConfigValidationMiddleware>();

app.MapControllers();

await app.RunAsync();
