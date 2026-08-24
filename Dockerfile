# syntax=docker/dockerfile:1

# ---------- Stage 1: Build Angular UI ----------
FROM node:22-alpine AS ui-build
WORKDIR /ui
COPY TicTacToe-UI/package.json TicTacToe-UI/package-lock.json ./
RUN npm ci
COPY TicTacToe-UI/ ./
RUN npm run build

# ---------- Stage 2: Build .NET API ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS api-build
WORKDIR /src
COPY TicTacToe.Api/TicTacToe.Api.csproj TicTacToe.Api/
RUN dotnet restore TicTacToe.Api/TicTacToe.Api.csproj
COPY TicTacToe.Api/ TicTacToe.Api/
RUN dotnet publish TicTacToe.Api/TicTacToe.Api.csproj -c Release -o /app/publish --no-restore

# Drop in the freshly built Angular bundle
COPY --from=ui-build /ui/dist/TicTacToe-UI/browser/ /app/publish/wwwroot/

# ---------- Stage 3: Runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Published API (TicTacToe.Api.dll) + Angular SPA already inside wwwroot/
COPY --from=api-build /app/publish .

EXPOSE 10000

ENV ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_EnableDiagnostics=0

# Bind Kestrel to Render's injected $PORT (fallback 10000 for local runs),
# then hand off to dotnet via exec so SIGTERM reaches the app.
ENTRYPOINT ["/bin/sh", "-c", "export ASPNETCORE_HTTP_PORTS=\"${PORT:-10000}\"; exec dotnet TicTacToe.Api.dll"]
