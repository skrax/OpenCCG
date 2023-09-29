FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["sources/OpenCCG.Gameserver/OpenCCG.Gameserver.csproj", "sources/OpenCCG.Gameserver/"]
RUN dotnet restore "sources/OpenCCG.Gameserver/OpenCCG.Gameserver.csproj"
COPY . .
WORKDIR "/src/sources/OpenCCG.Gameserver"
RUN dotnet build "OpenCCG.Gameserver.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OpenCCG.Gameserver.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT /bin/bash
#ENTRYPOINT ["dotnet", "OpenCCG.Gameserver.dll"]
