using AdventureTime.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Swagger generator
// This tells your application to build API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Adventure Time API",
        Version = "v1",
        Description = "An API for managing Adventure Time episodes and analysis"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// The order here matters - middleware executes in the order it's added
if (app.Environment.IsDevelopment())
{
    // Enable middleware to serve generated Swagger as a JSON endpoint
    app.UseSwagger();
    
    // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.)
    app.UseSwaggerUI(options =>
    {
        // This defines where the Swagger JSON file can be found
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Adventure Time API V1");
        
        // If you want Swagger UI at the app's root, uncomment the next line
        // options.RoutePrefix = string.Empty;
    });
}

app.UseAuthorization();

// This must come after UseAuthorization but before Run
app.MapControllers();

app.Run();