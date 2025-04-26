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

# Указываем, что в контейнере слушаем 8080
EXPOSE 8080

COPY --from=build /app/out .
# Гарантированно читаем PORT и прокидываем в --urls
ENTRYPOINT ["dotnet", "dotnet_mongodb.dll", "--urls", "http://*:$PORT"]
