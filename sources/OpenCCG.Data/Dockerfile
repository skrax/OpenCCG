FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
RUN apt-get update
RUN apt-get -y install curl

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["sources/OpenCCG.Data/OpenCCG.Data.csproj", "sources/OpenCCG.Data/"]
RUN dotnet restore "sources/OpenCCG.Data/OpenCCG.Data.csproj"
COPY . .
WORKDIR "/src/sources/OpenCCG.Data"
RUN dotnet build "OpenCCG.Data.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OpenCCG.Data.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OpenCCG.Data.dll"]
