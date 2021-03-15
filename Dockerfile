# imagem base
FROM mcr.microsoft.com/dotnet/sdk:5.0-focal AS build-env

# copia os arquivos
COPY . /app		

# web api
WORKDIR /app/src/AS.Api
RUN dotnet publish -c Release -o out

# imagem base final
FROM mcr.microsoft.com/dotnet/aspnet:5.0

RUN ["apt-get", "update"]

# trago arquivos da compilacao
COPY --from=build-env /app /app

# sustenta o container
WORKDIR /app/src