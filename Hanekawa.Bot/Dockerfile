FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["nuget.config", ""]
COPY ["Hanekawa.Bot/Hanekawa.Bot.csproj", "Hanekawa.Bot/"]
COPY ["Hanekawa.Infrastructure/Hanekawa.Infrastructure.csproj", "Hanekawa.Infrastructure/"]
COPY ["Hanekawa.Application/Hanekawa.Application.csproj", "Hanekawa.Application/"]
COPY ["Hanekawa/Hanekawa.csproj", "Hanekawa/"]
RUN dotnet restore "Hanekawa.Bot/Hanekawa.Bot.csproj"
COPY . .
WORKDIR "/src/Hanekawa.Bot"
RUN dotnet build "Hanekawa.Bot.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Hanekawa.Bot.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Hanekawa.Bot.dll"]