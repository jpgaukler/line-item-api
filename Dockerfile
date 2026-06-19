ARG BUILD_CONFIGURATION=Release
ARG BUILD_VERSION=unknown

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION
ARG BUILD_VERSION
WORKDIR /src
COPY ["LineItem.Api/LineItem.Api.csproj", "LineItem.Api/"]
COPY ["LineItem.Models/LineItem.Models.csproj", "LineItem.Models/"]
COPY ["LineItem.Services/LineItem.Services.csproj", "LineItem.Services/"]
COPY ["LineItem.Repositories/LineItem.Repositories.csproj", "LineItem.Repositories/"]
COPY ["LineItem.Exceptions/LineItem.Exceptions.csproj", "LineItem.Exceptions/"]
COPY ["LineItem.BuildVersion/LineItem.BuildVersion.csproj", "LineItem.BuildVersion/"]
RUN dotnet restore "LineItem.Api/LineItem.Api.csproj"
COPY . .
WORKDIR /src/LineItem.Api
RUN dotnet build "LineItem.Api.csproj" \
    --property:BuildVersion=$BUILD_VERSION \
    --configuration $BUILD_CONFIGURATION \
    --no-restore

#FROM build AS test
#ARG BUILD_CONFIGURATION
#RUN dotnet test "LineItem.Test.Unit/LineItem.Test.Unit.csproj" --configuration $BUILD_CONFIGURATION --no-build --verbosity normal

FROM build AS publish
ARG BUILD_CONFIGURATION
RUN dotnet publish "LineItem.Api.csproj" \
    --configuration $BUILD_CONFIGURATION \
    --output /app/publish \
    --no-build

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
EXPOSE 8080
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LineItem.Api.dll"]
