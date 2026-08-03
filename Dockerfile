FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Proje yapýlarýný kopyala
COPY ["SmartTaskEngine.API/SmartTaskEngine.API.csproj", "SmartTaskEngine.API/"]
COPY ["SmartTaskEngine.Application/SmartTaskEngine.Application.csproj", "SmartTaskEngine.Application/"]
COPY ["SmartTaskEngine.Domain/SmartTaskEngine.Domain.csproj", "SmartTaskEngine.Domain/"]
COPY ["SmartTaskEngine.Infrastructure/SmartTaskEngine.Infrastructure.csproj", "SmartTaskEngine.Infrastructure/"]

# Baðýmlýlýklarý restore et
RUN dotnet restore "SmartTaskEngine.API/SmartTaskEngine.API.csproj"

# Tüm kaynak kodlarý kopyala ve API projesini publish et
COPY . .
WORKDIR "/src/SmartTaskEngine.API"
RUN dotnet publish "SmartTaskEngine.API.csproj" -c Release -o /app/out

# Çalýþma zamaný (Runtime) katmaný
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Render port ayarý
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SmartTaskEngine.API.dll"]