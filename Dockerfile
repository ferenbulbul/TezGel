# Build aşaması
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Çözüm dosyasını ve csproj dosyalarını kopyala
COPY *.sln .
COPY TezGel.Application/*.csproj TezGel.Application/
COPY TezGel.Domain/*.csproj TezGel.Domain/
COPY TezGel.Infrastructure/*.csproj TezGel.Infrastructure/
COPY TezGel.Persistence/*.csproj TezGel.Persistence/
COPY TezGel.API/*.csproj TezGel.API/

# NuGet paketlerini restore et
RUN dotnet restore

# Tüm projeyi kopyala ve publish et
COPY . .
WORKDIR /src/TezGel.API
RUN dotnet publish -c Release -o /app/publish

# Çalışacak runtime ortamı (hafif image)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Uygulamayı başlat
ENTRYPOINT ["dotnet", "TezGel.API.dll"]
