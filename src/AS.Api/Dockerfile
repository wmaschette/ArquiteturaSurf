#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["src/AS.Api/AS.Api.csproj", "src/AS.Api/"]
COPY ["src/AS.Infra.IoC/AS.Infra.IoC.csproj", "src/AS.Infra.IoC/"]
COPY ["src/AS.Infra.Bus/AS.Infra.Bus.csproj", "src/AS.Infra.Bus/"]
COPY ["src/AS.Domain/AS.Domain.csproj", "src/AS.Domain/"]
COPY ["src/AS.Infra.Shared/AS.Infra.Shared.csproj", "src/AS.Infra.Shared/"]
COPY ["src/AS.Service.Identity/AS.Service.Identity.csproj", "src/AS.Service.Identity/"]
COPY ["src/AS.Application/AS.Application.csproj", "src/AS.Application/"]
RUN dotnet restore "src/AS.Api/AS.Api.csproj"
COPY . .
WORKDIR "/src/src/AS.Api"
RUN dotnet build "AS.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AS.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AS.Api.dll"]