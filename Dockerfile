# Stage 1: сборка
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY dotnet_mongodb.sln .  
COPY *.csproj ./  
RUN dotnet restore

COPY . .  
RUN dotnet publish -c Release -o out

# Stage 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .
EXPOSE 8080
ENTRYPOINT ["dotnet", "dotnet_mongodb.dll", "--urls", "http://*:$PORT"]
