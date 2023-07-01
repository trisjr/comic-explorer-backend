using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;

namespace PublicApi.Config;

public static class ConfigSwagger
{
    private const string Name = "v1";
    private const string ApiTitle = "Comic Explorer API";
    private const string ApiVersion = "v1";
    private const string BearerDescription = "JWT Authorization header using the Bearer scheme.";
    private const string BearerName = "Authorization";
    private const ParameterLocation BearerLocation = ParameterLocation.Header;
    private const SecuritySchemeType BearerType = SecuritySchemeType.ApiKey;
    private const string BearerScheme = "Bearer";
    private const string BearerId = "Bearer";

    public static void Configure(IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc(Name, new OpenApiInfo { Title = ApiTitle, Version = ApiVersion });

            c.AddSecurityDefinition(
                BearerId,
                new OpenApiSecurityScheme
                {
                    Description = BearerDescription,
                    Name = BearerName,
                    In = BearerLocation,
                    Type = BearerType,
                    Scheme = BearerScheme
                }
            );

            c.AddSecurityRequirement(
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = BearerId
                            }
                        },
                        new List<string>()
                    }
                }
            );
        });

        services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
    }
}
