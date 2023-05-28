using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using refer_me_back_end;
using refer_me_back_end.Repositories;
using refer_me_back_end.Services.JWTAuthService;
using System.Text;
using static refer_me_back_end.Services.CosmosService.JobPostsCosmosService;
using static refer_me_back_end.Services.CosmosService.UserCosmosService;

async Task<UserCosmosDbService> InitializeUsersCosmosClientInstanceAsync(IConfigurationSection configurationSection)
{
    var databaseName = configurationSection["DatabaseName"];
    var containerName = configurationSection["ContainerName"];
    var account = configurationSection["Account"];

    // Using key vault to get Users Table Key
    var secretClient = new SecretClient(vaultUri: new Uri("https://rg-refer-me-kv.vault.azure.net/"), credential: new DefaultAzureCredential());
    var secretKey = secretClient.GetSecret("ReferMeDB-Users-Key");
    var key = secretKey.Value.Value;
    //var key = configurationSection["Key"];
    var client = new CosmosClient(account, key);
    var database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
    await database.Database.CreateContainerIfNotExistsAsync(containerName, "/id");
    var cosmosDbService = new UserCosmosDbService(client, databaseName, containerName);
    return cosmosDbService;
}

async Task<JobPostsCosmosDbService> InitializeJobPostsCosmosClientInstanceAsync(IConfigurationSection configurationSection)
{
    var databaseName = configurationSection["DatabaseName"];
    var containerName = configurationSection["ContainerName"];
    var account = configurationSection["Account"];

    // Using key vault to get JobPosts Table Key
    var secretClient = new SecretClient(vaultUri: new Uri("https://rg-refer-me-kv.vault.azure.net/"), credential: new DefaultAzureCredential());
    var secretKey = secretClient.GetSecret("ReferMeDB-JobPosts-Key");
    var key = secretKey.Value.Value;

    //var key = configurationSection["Key"];
    var client = new CosmosClient(account, key);
    var database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
    await database.Database.CreateContainerIfNotExistsAsync(containerName, "/id");
    var cosmosDbService = new JobPostsCosmosDbService(client, databaseName, containerName);
    return cosmosDbService;
}

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(c =>
{
    c.AddPolicy("AllowOrigin", options => options.AllowAnyOrigin().AllowAnyMethod()
    .AllowAnyHeader());
});
// Add services to the container.

builder.Services.AddControllers();

// For adding Jwt Authetication service
builder.Services.AddAuthentication(
                authOptions =>
                {
                    authOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    authOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                }).AddJwtBearer(jwtOptions =>
                {
                    // Using key vault to get JwtConfig Key
                    var secretClient = new SecretClient(vaultUri: new Uri("https://rg-refer-me-kv.vault.azure.net/"), credential: new DefaultAzureCredential());
                    var secretKey = secretClient.GetSecretAsync("JwtConfig-Key").Result;
                    var key = secretKey.Value.Value;
                    //var key = builder.Configuration.GetValue<string>("JwtConfig:Key");
                    var keyBytes = Encoding.ASCII.GetBytes(key);
                    jwtOptions.SaveToken = true;
                    jwtOptions.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                        ValidateLifetime = true,
                        ValidateAudience = false,
                        ValidateIssuer = false,
                    };
                });

builder.Services.AddAuthorization();
builder.Services.AddAuthentication();

// Adding JwtAuthentication
builder.Services.AddSingleton(typeof(IJwtTokenManager), typeof(JwtTokenManager));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "JWTToken_Auth_API",
        Version = "v1"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Adding Cosmos Service for Users
builder.Services.AddSingleton<IUserCosmosDbService>
    (InitializeUsersCosmosClientInstanceAsync(builder.Configuration.GetSection("UserCosmosDb")).GetAwaiter().GetResult());

// Adding Cosmos Service for Job Posts
builder.Services.AddSingleton<IJobPostsCosmosDbService>
    (InitializeJobPostsCosmosClientInstanceAsync(builder.Configuration.GetSection("JobPostsCosmosDb")).GetAwaiter().GetResult());

// Adding IUserRepositories
builder.Services.AddScoped<IUserRepositories, UserRepository>();

// Adding IJobPostsRepositories
builder.Services.AddScoped<IJobPostsRepositories, JobPostsRepositories>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Adding DBContext
builder.Services.AddDbContext<ReferMeDBContext>();

// Adding Redis Cache
builder.Services.AddDistributedRedisCache(options =>
{
    var secretClient = new SecretClient(vaultUri: new Uri("https://rg-refer-me-kv.vault.azure.net/"), credential: new DefaultAzureCredential());
    var secretKey = secretClient.GetSecret("ReferMe-Redis-Cache-Key");
    var connectionString = secretKey.Value.Value;

    options.Configuration = connectionString;
});

var app = builder.Build();

app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
