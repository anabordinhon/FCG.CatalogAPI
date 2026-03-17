FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["FCG.Catalog.API/FCG.Catalog.API.csproj", "FCG.Catalog.API/"]
COPY ["FCG.Catalog.Application/FCG.Catalog.Application.csproj", "FCG.Catalog.Application/"]
COPY ["FCG.Catalog.Domain/FCG.Catalog.Domain.csproj", "FCG.Catalog.Domain/"]
COPY ["FCG.Catalog.Infrastructure/FCG.Catalog.Infrastructure.csproj", "FCG.Catalog.Infrastructure/"]
RUN dotnet restore "FCG.Catalog.API/FCG.Catalog.API.csproj"

COPY . .
RUN dotnet build "FCG.Catalog.API/FCG.Catalog.API.csproj" -c $BUILD_CONFIGURATION --no-restore
RUN dotnet publish "FCG.Catalog.API/FCG.Catalog.API.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    --no-build \
    /p:UseAppHost=false

RUN dotnet tool install --global dotnet-ef --version 8.0.8
ENV PATH="${PATH}:/root/.dotnet/tools"
RUN dotnet ef migrations bundle \
    --project FCG.Catalog.Infrastructure/FCG.Catalog.Infrastructure.csproj \
    --startup-project FCG.Catalog.API/FCG.Catalog.API.csproj \
    --configuration Release \
    --no-build \
    --self-contained \
    -o /app/efbundle

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FCG.Catalog.API.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS migrations
WORKDIR /app
COPY --from=build /app/efbundle .
COPY --from=build /app/publish .
RUN chmod +x ./efbundle
ENTRYPOINT ["./efbundle"]