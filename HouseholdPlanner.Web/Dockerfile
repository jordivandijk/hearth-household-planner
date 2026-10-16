FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["HouseholdPlanner.Web.csproj", "./"]
RUN dotnet restore "HouseholdPlanner.Web.csproj"
COPY . .
RUN dotnet publish "HouseholdPlanner.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM nginx:alpine AS final
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY appsettings-from-env.sh /docker-entrypoint.d/40-appsettings-from-env.sh
RUN chmod +x /docker-entrypoint.d/40-appsettings-from-env.sh
EXPOSE 80
