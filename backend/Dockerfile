FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY PropertyManagement.Domain/PropertyManagement.Domain.csproj PropertyManagement.Domain/
COPY PropertyManagement.Application/PropertyManagement.Application.csproj PropertyManagement.Application/
COPY PropertyManagement.Infrastructure/PropertyManagement.Infrastructure.csproj PropertyManagement.Infrastructure/
COPY PropertyManagement.API/PropertyManagement.API.csproj PropertyManagement.API/
RUN dotnet restore PropertyManagement.API/PropertyManagement.API.csproj

COPY . .
RUN dotnet publish PropertyManagement.API/PropertyManagement.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "PropertyManagement.API.dll"]
