FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ikigai-api.API/ikigai-api.API.csproj", "ikigai-api.API/"]
COPY ["ikigai-api.Application/ikigai-api.Application.csproj", "ikigai-api.Application/"]
COPY ["ikigai-api.Domain/ikigai-api.Domain.csproj", "ikigai-api.Domain/"]
COPY ["ikigai-api.Infrastructure/ikigai-api.Infrastructure.csproj", "ikigai-api.Infrastructure/"]

RUN dotnet restore "ikigai-api.API/ikigai-api.API.csproj"

COPY . .

WORKDIR "/src/ikigai-api.API"
RUN dotnet build "ikigai-api.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ikigai-api.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "ikigai-api.API.dll"]