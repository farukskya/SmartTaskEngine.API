FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Proje dosyalarýný kopyala ve restore et
COPY ["SmartTaskEngine.API.csproj", "./"]
RUN dotnet restore

# Tüm kodlarý kopyala ve Release modunda publish et
COPY . .
RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Port ayarý (Render 8080 portunu dinler)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SmartTaskEngine.API.dll"]