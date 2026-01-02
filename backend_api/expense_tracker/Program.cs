var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddScoped<expense_tracker.Services.CsvImportService>();
builder.Services.AddScoped<expense_tracker.Services.TransactionQueryService>();

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


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("AllowAngular");
app.UseAuthorization();

app.MapGet("/", () => "Hello, world!");
app.MapControllers();

app.Run();
