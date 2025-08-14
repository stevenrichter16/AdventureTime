# This Dockerfile uses a multi-stage build pattern, which is like building your 
# application in a clean room, then only shipping the final product.
# This keeps the final image small and secure.

# Stage 1: Build Stage - This is our "workshop" where we compile the code
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the solution file and project files first
# We do this separately to leverage Docker's layer caching
# If these files don't change, Docker can reuse the cached layer
COPY AdventureTime.sln ./
COPY AdventureTime/AdventureTime.csproj ./AdventureTime/
COPY AdventureTime.Application/AdventureTime.Application.csproj ./AdventureTime.Application/
COPY AdventureTime.Infrastructure/AdventureTime.Infrastructure.csproj ./AdventureTime.Infrastructure/

# Restore dependencies - this downloads all NuGet packages
# We explicitly specify the solution file and add verbose logging to catch issues
# The --disable-parallel flag prevents race conditions in package restoration
RUN dotnet restore AdventureTime.sln --disable-parallel

# Now copy all the source code
COPY . .

# Build the application in Release mode
# We remove the --no-restore flag and let it restore again if needed
# This ensures all packages, including analyzers, are properly available
WORKDIR /src/AdventureTime
RUN dotnet build -c Release

# Publish the application to a folder
# This creates a self-contained deployment with everything needed to run
# We let publish handle its own restore to ensure all runtime packages are available
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime Stage - This is our "shipping container"
# We use a smaller runtime image since we don't need the SDK anymore
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install curl for health checks and the debugger
RUN apt-get update && \
    apt-get install -y curl && \
    curl -sSL https://aka.ms/getvsdbgsh | /bin/sh /dev/stdin -v latest -l /vsdbg && \
    rm -rf /var/lib/apt/lists/*

# Copy the published application from the build stage
COPY --from=build /app/publish .

# Create a non-root user to run the application (security best practice)
RUN useradd -m -u 1001 appuser && chown -R appuser:appuser /app
USER appuser

# Expose port 8080 (we'll use this instead of 5256 for containerized apps)
# Using 8080 is a common convention for containerized web applications
EXPOSE 8080

# Set ASP.NET Core to listen on port 8080 for all interfaces
# The 0.0.0.0 binding is crucial - it allows connections from outside the container
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check - Docker will periodically check if the app is responsive
# This helps with orchestration and automatic restarts if needed
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Start the application
ENTRYPOINT ["dotnet", "AdventureTime.dll"]