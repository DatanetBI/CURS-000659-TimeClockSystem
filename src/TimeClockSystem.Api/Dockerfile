# syntax=docker/dockerfile:1
# Build de contenedor para TimeClockSystem.Api. Se construye con el contexto en la raíz del
# repositorio (docker-compose.yml lo hace así) para poder copiar los proyectos referenciados
# (Application, Domain, Infrastructure).

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/TimeClockSystem.Api/TimeClockSystem.Api.csproj", "src/TimeClockSystem.Api/"]
COPY ["src/TimeClockSystem.Application/TimeClockSystem.Application.csproj", "src/TimeClockSystem.Application/"]
COPY ["src/TimeClockSystem.Domain/TimeClockSystem.Domain.csproj", "src/TimeClockSystem.Domain/"]
COPY ["src/TimeClockSystem.Infrastructure/TimeClockSystem.Infrastructure.csproj", "src/TimeClockSystem.Infrastructure/"]
RUN dotnet restore "src/TimeClockSystem.Api/TimeClockSystem.Api.csproj"

COPY src/TimeClockSystem.Api/ src/TimeClockSystem.Api/
COPY src/TimeClockSystem.Application/ src/TimeClockSystem.Application/
COPY src/TimeClockSystem.Domain/ src/TimeClockSystem.Domain/
COPY src/TimeClockSystem.Infrastructure/ src/TimeClockSystem.Infrastructure/

RUN dotnet publish "src/TimeClockSystem.Api/TimeClockSystem.Api.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TimeClockSystem.Api.dll"]
