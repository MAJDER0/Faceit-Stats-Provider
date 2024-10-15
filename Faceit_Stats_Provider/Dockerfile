# Use ASP.NET base image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["Faceit_Stats_Provider/Faceit_Stats_Provider.csproj", "Faceit_Stats_Provider/"]
RUN dotnet restore "Faceit_Stats_Provider/Faceit_Stats_Provider.csproj"

# Copy the entire solution to the container
COPY . .

# Build the project
WORKDIR "/src/Faceit_Stats_Provider"
RUN dotnet build "Faceit_Stats_Provider.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Faceit_Stats_Provider.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app

# Copy the published app to the final stage
COPY --from=publish /app/publish .

# Ensure the wwwroot directory with robots.txt and sitemap.xml is included
COPY ["Faceit_Stats_Provider/wwwroot", "./wwwroot"]

# Run the application
ENTRYPOINT ["dotnet", "Faceit_Stats_Provider.dll"]
