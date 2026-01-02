using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddScoped<expense_tracker.Services.CsvImportService>();
builder.Services.AddScoped<expense_tracker.Services.TransactionQueryService>();

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
                  .SetIsOriginAllowed(_to => true)
                  .AllowAnyHeader()
                  .AllowAnyMethod(); 
                  
        });
});


var app = builder.Build();

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

app.UseHttpsRedirection();

app.UseCors("AllowAngular");
app.UseAuthorization();

app.MapGet("/", () => "Expense tracker is running");
app.MapControllers();

app.Run();
