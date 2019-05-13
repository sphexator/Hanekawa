FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["Hanekawa.Web/Hanekawa.Web.csproj", "Hanekawa.Web/"]
RUN dotnet restore "Hanekawa.Web/Hanekawa.Web.csproj"
COPY . .
WORKDIR "/src/Hanekawa.Web"
RUN dotnet build "Hanekawa.Web.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Hanekawa.Web.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Hanekawa.Web.dll"]