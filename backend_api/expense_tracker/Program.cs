// using expense_tracker.Services;
// using expense_tracker.Services.Interfaces;
// using Microsoft.AspNetCore.Builder;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Hosting;
// using Dapper;
using System.Security.Cryptography;
using expense_tracker.Services;
using expense_tracker.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

//
// Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddScoped<expense_tracker.Services.CsvImportService>();
builder.Services.AddScoped<expense_tracker.Services.TransactionQueryService>();

builder.Services.AddScoped<expense_tracker.Services.AuthService>();
builder.Services.AddScoped<expense_tracker.Services.TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
//builder.Services.AddOpenApi(); maybe for later versions

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();


var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy
                   //WithOrigins(allowedOrigins)  # after I setup angular hosting
                  .SetIsOriginAllowed(to => true)
                  .AllowAnyHeader()
                  .AllowAnyMethod(); 
                  
        });
});

var publicKeyPem = File.ReadAllText("Utils/Keys/jwt_public.pem");
var rsa = RSA.Create();
rsa.ImportFromPem(publicKeyPem);

var key = new RsaSecurityKey(rsa);

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key
        };
    });



//swagger stuff
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});

//below builds app
var app = builder.Build();
app.UseHttpsRedirection();
app.UseCors("AllowAngular");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Will change to just development on later builds
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => "Expense tracker is running");
app.MapControllers();

app.Run();
