FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Hanekawa.Bot/Hanekawa.Bot.csproj", "Hanekawa.Bot/"]
RUN dotnet restore "Hanekawa.Bot/Hanekawa.Bot.csproj"
COPY . .
WORKDIR "/src/Hanekawa.Bot"
RUN dotnet build "Hanekawa.Bot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Hanekawa.Bot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Hanekawa.Bot.dll"]
