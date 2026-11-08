FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Domain/Domain.csproj Domain/
COPY src/Shared/Shared.csproj Shared/
COPY src/Application/Application.csproj Application/
COPY src/Infra/Infra.csproj Infra/
COPY src/API/API.csproj API/
RUN dotnet restore API/API.csproj

COPY src/ .
RUN dotnet publish API/API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "API.dll"]
