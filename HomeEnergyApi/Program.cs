using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.OpenApi.Models;
using HomeEnergyApi.Models;
using HomeEnergyApi.Services;
using HomeEnergyApi.Dtos;
using HomeEnergyApi.Filters;
using HomeEnergyApi.Authorization;
using HomeEnergyApi.Security;
using HomeEnergyApi.Middleware;
using HomeEnergyApi.Wrapper;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging();

builder.Services.AddScoped<HomeRepository>();
builder.Services.AddScoped<IReadRepository<int, Home>>(provider => provider.GetRequiredService<HomeRepository>());
builder.Services.AddScoped<IWriteRepository<int, Home>>(provider => provider.GetRequiredService<HomeRepository>());
builder.Services.AddScoped<IOwnerLastNameQueryable<Home>>(provider => provider.GetRequiredService<HomeRepository>());
builder.Services.AddScoped<IPaginatedReadRepository<int, Home>>(provider => provider.GetRequiredService<HomeRepository>());

builder.Services.AddScoped<UtilityProviderRepository>();
builder.Services.AddScoped<IReadRepository<int, UtilityProvider>>(provider => provider.GetRequiredService<UtilityProviderRepository>());
builder.Services.AddScoped<IWriteRepository<int, UtilityProvider>>(provider => provider.GetRequiredService<UtilityProviderRepository>());

builder.Services.AddScoped<HomeUtilityProviderRepository>();
builder.Services.AddScoped<IReadRepository<int, HomeUtilityProvider>>(provider => provider.GetRequiredService<HomeUtilityProviderRepository>());
builder.Services.AddScoped<IWriteRepository<int, HomeUtilityProvider>>(provider => provider.GetRequiredService<HomeUtilityProviderRepository>());

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddSingleton<ValueHasher>();
builder.Services.AddSingleton<ValueEncryptor>();

builder.Services.AddTransient<ZipCodeLocationService>();
builder.Services.AddHttpClient<ZipCodeLocationService>();

builder.Services.AddTransient<HomeUtilityProviderService>();

builder.Services.AddSingleton<RateLimitingService>();
builder.Services.AddSingleton<IDateTimeWrapper, DateTimeWrapper>();

builder.Services.AddSingleton<DecryptionAuditService>();
builder.Services.AddSingleton<DecryptionLoggingService>();

builder.Services.AddDbContext<HomeDbContext>(options =>
    options.UseSqlite("Data Source=Homes.db").ConfigureWarnings(warings =>
    warings.Ignore(RelationalEventId.NonTransactionalMigrationOperationWarning)));

builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
});

builder.Services.AddAutoMapper(typeof(HomeProfile));

builder.Configuration.AddJsonFile("secrets.json");
        
builder.Services.AddAuthentication("JwtAuthentication")
    .AddScheme<AuthenticationSchemeOptions, JwtAuthenticationHandler>("JwtAuthentication", null);

builder.Services.AddAuthorization(options => 
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
});

builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});


builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Home Energy Api V1", Version = "v1" });
    options.SwaggerDoc("v2", new OpenApiInfo { Title = "Home Energy Api V2", Version = "v2" });

    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "X-Api-Key",
        Type = SecuritySchemeType.ApiKey,
        Description = "API Key from header",
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                },
                In = ParameterLocation.Header,
            },
            new List<string>()
        },
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new List<string>() // No specific scopes
        }
    });
});

builder.Services.AddMemoryCache();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HomeDbContext>();
    db.Database.Migrate();

    var encryptor = scope.ServiceProvider.GetRequiredService<ValueEncryptor>();
    var decryptionAuditService = scope.ServiceProvider.GetRequiredService<DecryptionAuditService>();
    var decryptionLoggingService = scope.ServiceProvider.GetRequiredService<DecryptionLoggingService>();

    encryptor.ValueDecrypted += decryptionAuditService.OnValueDecrypted;
    encryptor.ValueDecrypted += decryptionLoggingService.OnValueDecrypted;
}

app.UseMiddleware<RateLimitingMiddleware>();

app.UseWhen(context => !context.Request.Path.StartsWithSegments("/swagger"), appBuilder =>
{
    appBuilder.UseMiddleware<ApiKeyMiddleware>();
    appBuilder.UseAuthentication();
    appBuilder.UseAuthorization();
});

app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Home Energy Api V1");
    c.SwaggerEndpoint("/swagger/v2/swagger.json", "Home Energy Api V2");
    c.RoutePrefix = "swagger";
});

app.Run();

//Do NOT remove anything below this comment, this is required to autograde the lesson
public partial class Program { }