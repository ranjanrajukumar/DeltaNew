using Microsoft.OpenApi;

namespace SchoolERP.API.Extensions;

/// <summary>
/// API documentation: the OpenAPI document at /openapi/v1.json and Swagger UI at /swagger.
/// Sign in with POST /api/auth/login, then paste the token into "Authorize".
/// </summary>
public static class SwaggerExtensions
{
    private const string Scheme = "Bearer";

    public static IServiceCollection AddSchoolErpOpenApi(this IServiceCollection services) =>
        services.AddOpenApi(options =>
        {
            options.AddSchemaTransformer((schema, context, _) =>
            {
                if (context.JsonTypeInfo.Type == typeof(System.Text.Json.Nodes.JsonObject))
                {
                    schema.Type = JsonSchemaType.Object;
                    schema.Description = "JSON request body. See the endpoint documentation for accepted fields.";
                    schema.Example = System.Text.Json.Nodes.JsonNode.Parse("{}");
                }
                return Task.CompletedTask;
            });
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Info = new OpenApiInfo
                {
                    Title = "SchoolERP API",
                    Version = "v1",
                    Description = "School ERP API. Sign in with POST /api/auth/login, then click Authorize and paste the token.",
                };
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                document.Components.SecuritySchemes[Scheme] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "The token from POST /api/auth/login (valid JwtSettings:ExpiryMinutes).",
                };
                document.Security ??= [];
                document.Security.Add(new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference(Scheme, document)] = [] });
                return Task.CompletedTask;
            });
        });

    public static WebApplication UseSchoolErpSwagger(this WebApplication app)
    {
        app.MapOpenApi(); // /openapi/v1.json
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/v1.json", "SchoolERP API v1");
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "SchoolERP API";
            options.EnablePersistAuthorization(); // keep the token across page reloads
            options.DisplayRequestDuration();
            options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
            options.DefaultModelsExpandDepth(2);
        });
        return app;
    }
}
