# Estágio de Build utilizando o SDK oficial do .NET 10
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia os arquivos de definição de projeto para restaurar as dependências
COPY ["RepCortex.sln", "./"]
COPY ["RepCortex.API/RepCortex.API.csproj", "RepCortex.API/"]
COPY ["RepCortex.Tests/RepCortex.Tests.csproj", "RepCortex.Tests/"]

# Executa o restore isolado (otimiza o cache de camadas do Docker)
RUN dotnet restore

# Copia todo o restante do código fonte e compila
COPY . .
WORKDIR "/src/RepCortex.API"
RUN dotnet publish "RepCortex.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio final de execução usando o Runtime enxuto do .NET 10
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Porta padrão exposta pelo container do ASP.NET Core
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "RepCortex.API.dll"]