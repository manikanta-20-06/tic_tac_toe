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
COPY --from=api-build /app/publish .

# Render injects PORT; Kestrel must listen on it (defaults to 10000 locally)
ENTRYPOINT ["sh", "-c", "export ASPNETCORE_HTTP_PORTS=${PORT:-10000}; exec dotnet TicTacToe.Api.dll"]
