FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["FourthLabThirdTask/FourthLabThirdTask.csproj", "FourthLabThirdTask/"]
RUN dotnet restore "FourthLabThirdTask/FourthLabThirdTask.csproj"
COPY . .
WORKDIR "/src/FourthLabThirdTask"
RUN dotnet build "FourthLabThirdTask.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FourthLabThirdTask.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FourthLabThirdTask.dll"]
