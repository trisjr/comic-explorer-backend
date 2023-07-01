using System.Text;
using ApplicationCore.Constants;
using ApplicationCore.Entities.User;
using Infrastructure.User;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using PublicApi.Config;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();

// Add services to the container
ConfigService.Configure(builder.Configuration, builder.Services);
builder.Services.AddMemoryCache();

var key = Encoding.UTF8.GetBytes(builder.Configuration["JWTSecretKey"]);
builder.Services
    .AddAuthentication(config =>
    {
        config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        config.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(config =>
    {
        config.RequireHttpsMetadata = false;
        config.SaveToken = true;
        config.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

const string corsPolicy = "CorsPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        corsPolicy,
        corsPolicyBuilder =>
        {
            corsPolicyBuilder.AllowAnyMethod();
            corsPolicyBuilder.AllowAnyHeader();
            corsPolicyBuilder.AllowAnyOrigin();
        }
    );
});

builder.Services.AddControllers();
builder.Configuration.AddEnvironmentVariables();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

ConfigSwagger.Configure(builder.Services);
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = PasswordSettings.RequireDigit;
    options.Password.RequireLowercase = PasswordSettings.RequireLowercase;
    options.Password.RequireNonAlphanumeric = PasswordSettings.RequireNonAlphanumeric;
    options.Password.RequireUppercase = PasswordSettings.RequireUppercase;
    options.Password.RequiredLength = PasswordSettings.RequiredLength;
    options.Password.RequiredUniqueChars = PasswordSettings.RequiredUniqueChars;
});

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var scopedProvider = scope.ServiceProvider;
    try
    {
        var userManager = scopedProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scopedProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var identityContext = scopedProvider.GetRequiredService<AppIdentityDbContext>();
        await AppIdentityDbContextSeed.SeedAsync(identityContext, userManager, roleManager);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred seeding the DB");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors(corsPolicy);

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "AVT Api"); });

app.MapControllers();

app.Run();
