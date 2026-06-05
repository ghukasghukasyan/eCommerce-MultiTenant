FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY eCommerce.sln ./
COPY eCommerce.Frontend/eCommerce.Frontend.csproj eCommerce.Frontend/
COPY ClientLibrary/ClientLibrary.csproj ClientLibrary/
RUN dotnet restore eCommerce.Frontend/eCommerce.Frontend.csproj

COPY . .
RUN dotnet publish eCommerce.Frontend/eCommerce.Frontend.csproj -c Release -o /app/publish

FROM nginx:1.27-alpine AS runtime
COPY eCommerce.Frontend/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
EXPOSE 80 443