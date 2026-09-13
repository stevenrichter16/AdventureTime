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
            // Allow the viewer from any local origin (localhost or 127.0.0.1, any port).
            // Vite picks a new port if 5173 is busy, and Safari reports a blocked
            // cross-origin fetch as a bare "Load failed", so be permissive for local dev.
            policy.SetIsOriginAllowed(origin =>
                    Uri.TryCreate(origin, UriKind.Absolute, out var uri) && uri.IsLoopback)
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

// Liveness endpoint used by the Docker HEALTHCHECK
builder.Services.AddHealthChecks();

// Register Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
app.MapHealthChecks("/health");

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