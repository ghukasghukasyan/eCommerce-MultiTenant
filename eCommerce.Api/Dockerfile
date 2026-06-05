FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY eCommerce.sln ./
COPY eCommerce.Api/eCommerce.Api.csproj eCommerce.Api/
COPY eCommerce.Application/eCommerce.Application.csproj eCommerce.Application/
COPY eCommerce.Infrastructure/eCommerce.Infrastructure.csproj eCommerce.Infrastructure/
COPY eCommerce.Domain/eCommerce.Domain.csproj eCommerce.Domain/
RUN dotnet restore eCommerce.Api/eCommerce.Api.csproj

COPY . .
RUN dotnet publish eCommerce.Api/eCommerce.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

COPY eCommerce.Api/entrypoint.sh /app/entrypoint.sh
RUN chmod +x /app/entrypoint.sh

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["/app/entrypoint.sh"]
