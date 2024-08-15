using System.Reflection;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using CityInfo.API.DbContexts;
using CityInfo.API.Services;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Set up logger
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/city-info.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

/*
 Default logging configuration
 builder.Logging
    .ClearProviders()
    .AddConsole();*/
builder.Host.UseSerilog();

// Set up serializers
builder.Services.AddControllers(options => { options.ReturnHttpNotAcceptable = true; })
    .AddNewtonsoftJson()
    .AddXmlSerializerFormatters();

// Support for serving static files
builder.Services.AddSingleton<FileExtensionContentTypeProvider>();

// Add custom services
#if DEBUG
builder.Services.AddTransient<IMailService, LocalMailService>();
#else
builder.Services.AddTransient<IMailService, CloudMailService>();
#endif

// Customize error responses format
builder.Services.AddProblemDetails();
/*
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = ctx =>
    {
        ctx.ProblemDetails.Extensions.Add("additionalInfo", "Additional Info Example");
        ctx.ProblemDetails.Extensions.Add("server", Environment.MachineName);
    };
});*/

// Configure EF and repositories
builder.Services.AddDbContext<CityInfoDbContext>(dbContextOptions =>
    dbContextOptions.UseSqlite(builder.Configuration["ConnectionStrings:CityInfoDbConnectionString"]));
builder.Services.AddScoped<ICityInfoRepository, CityInfoRepository>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Security configurations
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Authentication:Issuer"],
                ValidAudience = builder.Configuration["Authentication:Audience"],
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Convert.FromBase64String(builder.Configuration["Authentication:SecretKey"]))
            };
        }
    );

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MustBeFromPrague", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("city", "Prague");
    });
});

// Api versioning
builder.Services.AddApiVersioning(setupAction =>
{
    setupAction.ReportApiVersions = true;
    setupAction.AssumeDefaultVersionWhenUnspecified = true;
    setupAction.DefaultApiVersion = new ApiVersion(1, 0);
}).AddMvc().AddApiExplorer(setupAction =>
{
    setupAction.SubstituteApiVersionInUrl = true;
});

// Configure Swagger/OpenAPI
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
var apiVersionDescriptionProvider =
    builder.Services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(setupAction =>
{
    foreach (var apiVersionDescription in apiVersionDescriptionProvider.ApiVersionDescriptions)
    {
        setupAction.SwaggerDoc($"{apiVersionDescription.GroupName}",
            new()
            {
                Title = "CityInfo API", 
                Version = apiVersionDescription.ApiVersion.ToString(),
                Description = "Through this API you can access information about cities and their points of interest.",
            });
    }
    
    var xmlCommentsFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlCommentsFullPath = Path.Combine(AppContext.BaseDirectory, xmlCommentsFile);
            
    setupAction.IncludeXmlComments(xmlCommentsFullPath);
    setupAction.AddSecurityDefinition("CityInfoBearedAuth", new()
    {
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        Description = "Input a valid JWT token to access this API."
    });
    setupAction.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new OpenApiReference()
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "CityInfoBearedAuth"
                }
            },
            new List<String>()
        }
    });
    
});


var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(setupAction =>
    {
        var descriptions = app.DescribeApiVersions();
        foreach (var apiVersionDescription in descriptions)
        {
            setupAction.SwaggerEndpoint(
                $"/swagger/{apiVersionDescription.GroupName}/swagger.json",
                apiVersionDescription.GroupName.ToUpperInvariant()
                );
        }
    });
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();