using Feijuca.Auth.Common.Models;
using Feijuca.Auth.Extensions;
using Feijuca.Auth.Infra.CrossCutting.Extensions;
using Feijuca.Auth.Infra.CrossCutting.Middlewares;
using Mattioli.Configurations.Extensions.Handlers;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
var enviroment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

builder.Configuration
    .AddJsonFile("appsettings.json", false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{enviroment}.json", true, reloadOnChange: true)
    .AddEnvironmentVariables();

var applicationSettings = builder.Configuration.ApplyEnvironmentOverridesToSettings(builder.Environment);

builder.Services
    .AddExceptionHandler<GlobalExceptionHandler>()
    .AddProblemDetails()
    .ConfigureLiteBus()
    .AddRepositories()
    .AddValidators()
    .AddServices()
    .AddOpenApi("v1")
    .AddMongo(applicationSettings.MongoSettings)
    .AddApiAuthentication(out KeycloakSettings keycloakSettings)
    .AddHealthCheckers(keycloakSettings)
    .AddEndpointsApiExplorer()
    .AddSwagger(keycloakSettings)
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
app.MapScalarApiReference(options => options.Servers = []);
app.UseRouting();
app.UseCors("AllowAllOrigins");
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.UseTenantMiddleware();
app.UseMiddleware<ConfigValidationMiddleware>();
app.UseHealthCheckers(keycloakSettings);
app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Feijuca.Auth.Api");
});

app.MapControllers();

await app.RunAsync();
