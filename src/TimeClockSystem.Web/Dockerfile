# syntax=docker/dockerfile:1
# Build de contenedor para TimeClockSystem.Web. Se construye con el contexto en la raíz del
# repositorio (docker-compose.yml lo hace así). El proyecto no referencia ningún otro (consume el
# Backend exclusivamente vía HTTP), por lo que solo se copia a sí mismo.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/TimeClockSystem.Web/TimeClockSystem.Web.csproj", "src/TimeClockSystem.Web/"]
RUN dotnet restore "src/TimeClockSystem.Web/TimeClockSystem.Web.csproj"

COPY src/TimeClockSystem.Web/ src/TimeClockSystem.Web/

RUN dotnet publish "src/TimeClockSystem.Web/TimeClockSystem.Web.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TimeClockSystem.Web.dll"]
