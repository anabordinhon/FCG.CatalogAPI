FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["FCG.Catalog.API/FCG.Catalog.API.csproj", "FCG.Catalog.API/"]
COPY ["FCG.Catalog.Application/FCG.Catalog.Application.csproj", "FCG.Catalog.Application/"]
COPY ["FCG.Catalog.Domain/FCG.Catalog.Domain.csproj", "FCG.Catalog.Domain/"]
COPY ["FCG.Catalog.Infrastructure/FCG.Catalog.Infrastructure.csproj", "FCG.Catalog.Infrastructure/"]
RUN dotnet restore "FCG.Catalog.API/FCG.Catalog.API.csproj"
COPY . .
WORKDIR "/src/FCG.Catalog.API"
RUN dotnet publish "FCG.Catalog.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FCG.Catalog.API.dll"]

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS migrations
WORKDIR /src
COPY . .
RUN dotnet tool install --global dotnet-ef --version 8.0.8
ENV PATH="${PATH}:/root/.dotnet/tools"
ENTRYPOINT ["dotnet", "ef", "database", "update", "--project", "FCG.Catalog.Infrastructure/FCG.Catalog.Infrastructure.csproj", "--startup-project", "FCG.Catalog.API/FCG.Catalog.API.csproj"]