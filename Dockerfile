# Etapa 1: Build da aplicação
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar o arquivo .csproj e restaurar as dependências
COPY *.csproj ./
RUN dotnet restore

# Copiar o restante do código do projeto e compilar
COPY . ./
RUN dotnet publish -c Release -o out

# Etapa 2: Configuração do ambiente de runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Expor a porta padrão do ASP.NET Core
EXPOSE 80

# Definir o ponto de entrada do contêiner
ENTRYPOINT ["dotnet", "MarlinIdiomasAPI.dll"]
