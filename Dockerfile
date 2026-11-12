
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Gatherly.API/Gatherly.API.csproj", "Gatherly.API/"]
COPY ["Gatherly.Core/Gatherly.Core.csproj", "Gatherly.Core/"]
COPY ["Gatherly.Data/Gatherly.Data.csproj", "Gatherly.Data/"]
COPY ["Gatherly.Services/Gatherly.Services.csproj", "Gatherly.Services/"]

RUN dotnet restore "Gatherly.API/Gatherly.API.csproj"

COPY . .

WORKDIR /src/Gatherly.API
RUN dotnet publish "Gatherly.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "Gatherly.API.dll"]
