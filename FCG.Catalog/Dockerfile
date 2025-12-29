FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

RUN dotnet tool install --global dotnet-ef --version 8.0.0
ENV PATH="${PATH}:/root/.dotnet/tools"

COPY ["FCG.Catalog.API/FCG.Catalog.API.csproj", "FCG.Catalog.API/"]
COPY ["FCG.Catalog.Application/FCG.Catalog.Application.csproj", "FCG.Catalog.Application/"]
COPY ["FCG.Catalog.Domain/FCG.Catalog.Domain.csproj", "FCG.Catalog.Domain/"]
COPY ["FCG.Catalog.Infrastructure/FCG.Catalog.Infrastructure.csproj", "FCG.Catalog.Infrastructure/"]

RUN dotnet restore "FCG.Catalog.API/FCG.Catalog.API.csproj"

COPY . .
WORKDIR "/src/FCG.Catalog.API"
RUN dotnet build "FCG.Catalog.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FCG.Catalog.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=build /root/.dotnet/tools /root/.dotnet/tools
ENV PATH="${PATH}:/root/.dotnet/tools"

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "FCG.Catalog.API.dll"]