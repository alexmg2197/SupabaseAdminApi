# Etapa 1: build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiamos csproj y restauramos dependencias
COPY *.csproj ./
RUN dotnet restore

# Copiamos todo y publicamos
COPY . ./
RUN dotnet publish -c Release -o out

# Etapa 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Expone el puerto 80
EXPOSE 80
ENTRYPOINT ["dotnet", "SupabaseAdminApi.dll"]
