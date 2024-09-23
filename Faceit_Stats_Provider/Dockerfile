# Use the official ASP.NET Core runtime as a base image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
# Copy the .csproj and restore as distinct layers
COPY ["Faceit_Stats_Provider.csproj", "./"]
RUN dotnet restore "./Faceit_Stats_Provider.csproj"
# Copy everything else and build
COPY . .
WORKDIR "/src"
RUN dotnet build "./Faceit_Stats_Provider.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "./Faceit_Stats_Provider.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Create the final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Faceit_Stats_Provider.dll"]
