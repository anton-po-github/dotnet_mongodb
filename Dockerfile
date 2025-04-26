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

# Говорим, что контейнер слушает порт 8080
EXPOSE 8080

# Копируем собранное приложение
COPY --from=build /app/out .  

# Явно задаём --urls из переменной $PORT
ENTRYPOINT ["dotnet", "dotnet_mongodb.dll", "--urls", "http://*:$PORT"]
