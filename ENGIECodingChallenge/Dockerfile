#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ENGIECodingChallenge/ENGIECodingChallenge.csproj", "ENGIECodingChallenge/"]
RUN dotnet restore "ENGIECodingChallenge/ENGIECodingChallenge.csproj"
COPY . .
WORKDIR "/src/ENGIECodingChallenge"
RUN dotnet build "ENGIECodingChallenge.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ENGIECodingChallenge.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ENGIECodingChallenge.dll"]