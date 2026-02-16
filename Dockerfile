FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Tasky.Api/Tasky.Api.csproj", "src/Tasky.Api/"]
COPY ["src/Tasky.Domain/Tasky.Domain.csproj", "src/Tasky.Domain/"]
COPY ["src/Tasky.Application/Tasky.Application.csproj", "src/Tasky.Application/"]
COPY ["src/Tasky.Infrastructure/Tasky.Infrastructure.csproj", "src/Tasky.Infrastructure/"]
RUN dotnet restore "src/Tasky.Api/Tasky.Api.csproj"
COPY . .
WORKDIR "/src/src/Tasky.Api"
RUN dotnet build "Tasky.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Tasky.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tasky.Api.dll"]
