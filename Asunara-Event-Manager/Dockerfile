FROM mcr.microsoft.com/dotnet/runtime:9.0-alpine AS base
RUN apk add libsodium opus
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Asunara-Event-Manager/Asunara-Event-Manager.csproj", "Asunara-Event-Manager/"]
RUN dotnet restore "Asunara-Event-Manager/Asunara-Event-Manager.csproj"
COPY . .
WORKDIR "/src/Asunara-Event-Manager"
RUN dotnet build "./Asunara-Event-Manager.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Asunara-Event-Manager.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Asunara-Event-Manager.dll"]
