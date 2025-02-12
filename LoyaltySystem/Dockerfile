FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["LoyaltySystem/LoyaltySystem.csproj", "LoyaltySystem/"]
RUN dotnet restore "LoyaltySystem/LoyaltySystem.csproj"
COPY . .
WORKDIR "/src/LoyaltySystem"
RUN dotnet build "LoyaltySystem.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LoyaltySystem.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LoyaltySystem.dll"]
