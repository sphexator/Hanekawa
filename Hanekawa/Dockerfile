FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80

ENV token=""
ENV connectionString=""

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["nuget.config", ""]
COPY ["Hanekawa/Hanekawa.csproj", "Hanekawa/"]
COPY ["Hanekawa.Database/Hanekawa.Database.csproj", "Hanekawa.Database/"]
COPY ["Hanekawa.Shared/Hanekawa.Shared.csproj", "Hanekawa.Shared/"]
COPY ["Hanekawa.AnimeSimulCast/Hanekawa.AnimeSimulCast.csproj", "Hanekawa.AnimeSimulCast/"]
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