FROM mcr.microsoft.com/dotnet/aspnet:7.0-bookworm-slim-amd64 AS base
WORKDIR /app
EXPOSE 80

RUN apt update && apt-get install -y postgresql-15 postgresql-client-15

RUN apt-get update && apt-get install -y tzdata
ENV TZ=Europe/Berlin
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

#ENV ASPNETCORE_HTTP_PORTS=80
ENV ASPNETCORE_URLS=http://+:80

FROM mcr.microsoft.com/dotnet/sdk:7.0-bookworm-slim-amd64 AS build
WORKDIR /src
COPY ["PostgresBackupTool/PostgresBackupTool.csproj", "PostgresBackupTool/"]
RUN dotnet restore "PostgresBackupTool/PostgresBackupTool.csproj"
COPY . .
WORKDIR "/src/PostgresBackupTool"
RUN dotnet build "PostgresBackupTool.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PostgresBackupTool.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PostgresBackupTool.dll"]
