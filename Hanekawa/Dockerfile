FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 61038

ENV token=""
ENV connectionString=""

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["nuget.config", ""]
COPY ["Hanekawa/Hanekawa.csproj", "Hanekawa/"]
COPY ["Hanekawa.Database/Hanekawa.Database.csproj", "Hanekawa.Database/"]
COPY ["Hanekawa.Shared/Hanekawa.Shared.csproj", "Hanekawa.Shared/"]
COPY ["Hanekawa.HungerGames/Hanekawa.HungerGames.csproj", "Hanekawa.HungerGames/"]
RUN dotnet restore "Hanekawa/Hanekawa.csproj"
COPY . .
WORKDIR "/src/Hanekawa"
RUN dotnet build "Hanekawa.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Hanekawa.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Hanekawa.dll"]