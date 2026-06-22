FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY RepCortex.API/RepCortex.API.csproj RepCortex.API/
COPY RepCortex.Tests/RepCortex.Tests.csproj RepCortex.Tests/
RUN dotnet restore RepCortex.API/RepCortex.API.csproj

# Copy everything else and publish
COPY . .
RUN dotnet publish RepCortex.API/RepCortex.API.csproj -c Release -o /app/out

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/out .

ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080}
ENTRYPOINT ["dotnet", "RepCortex.API.dll"]
