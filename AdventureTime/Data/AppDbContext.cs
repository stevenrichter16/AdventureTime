using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using AdventureTime.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AdventureTime.Data
{
    // The DbContext is like a bridge between your C# code and the database
    // It knows about all your tables and how to translate between objects and SQL
    public class AppDbContext : DbContext
    {
        // This constructor receives configuration options from ASP.NET Core's DI container
        // Think of it like receiving the keys and address to the database
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        
        // Each DbSet represents a table in your database
        // This tells EF Core: "I have a collection of Episodes that should be stored in a table"
        public DbSet<Episode> Episodes { get; set; }
        
        // OnModelCreating is where we fine-tune how our C# models map to database tables
        // It's like providing special filing instructions to our librarian
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var stringListConverter = new ListToJsonConverter<string>();
            modelBuilder.Entity<Episode>(entity =>
            {
                // Tell PostgreSQL that these columns should be JSONB type
                // This enables efficient JSON operations in the database
                entity.Property(e => e.MajorCharacters).HasColumnType("jsonb").HasConversion(stringListConverter);
                entity.Property(e => e.MinorCharacters).HasColumnType("jsonb").HasConversion(stringListConverter);
                entity.Property(e => e.Locations).HasColumnType("jsonb").HasConversion(stringListConverter);
                
                // Create a unique constraint: no two episodes can have the same season and episode number
                // This is like saying "each book can only have one spot on the shelf"
                entity.HasIndex(e => new { e.Season, e.EpisodeNumber })
                    .IsUnique()
                    .HasDatabaseName("IX_Episode_Season_Number");
            });
        }
    }
}

public class ListToJsonConverter<T> : ValueConverter<List<T>, string>
{
    public ListToJsonConverter() : base(
        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
        v => JsonSerializer.Deserialize<List<T>>(v, (JsonSerializerOptions)null) ?? new List<T>())
    {
    }
}