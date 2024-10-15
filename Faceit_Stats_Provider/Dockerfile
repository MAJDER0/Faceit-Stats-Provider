
#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images >

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Faceit_Stats_Provider/Faceit_Stats_Provider.csproj", "Faceit_Stats_Provider/"]
RUN dotnet restore "Faceit_Stats_Provider/Faceit_Stats_Provider.csproj"
COPY . .
WORKDIR "/src/Faceit_Stats_Provider"
RUN dotnet build "Faceit_Stats_Provider.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Faceit_Stats_Provider.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Faceit_Stats_Provider.dll"]
