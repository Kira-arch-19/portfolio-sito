
# Fase 1: compilazione ASP.NET Core
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore Portfolio.csproj

RUN dotnet publish Portfolio.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Fase 2: immagine di esecuzione
FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

COPY --from=build /app/publish .

# Prepara le directory persistenti
RUN mkdir -p /app/wwwroot/uploads /app/keys \
    && chown -R $APP_UID:$APP_UID /app/wwwroot/uploads /app/keys

USER $APP_UID

ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "Portfolio.dll"]

