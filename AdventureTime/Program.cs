using AdventureTime.Infrastructure;
using AdventureTime.Application;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173", "http://localhost:3000") // Vite uses 5173
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

// Add Infrastructure services
// This single line configures all database and external service dependencies
// The beauty is that your web project doesn't need to know about Entity Framework or PostgreSQL
builder.Services.AddInfrastructure(builder.Configuration);

// Add Application services
// We'll create this extension method next to keep things organized
builder.Services.AddApplication();

// Register Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Adventure Time API",
        Version = "v1",
        Description = "An API for managing Adventure Time episodes using Clean Architecture with CQRS pattern"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Adventure Time API V1");
    });
}

app.UseCors("AllowReactApp");

// Standard middleware pipeline
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Optional: Ensure database is created and migrations are applied
// This is helpful during development
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<AdventureTime.Infrastructure.Data.AppDbContext>();
        context.Database.EnsureCreated();
        // Or use migrations: await context.Database.MigrateAsync();
    }
}

app.Run();