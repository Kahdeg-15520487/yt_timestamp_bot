FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["NuGet.Config", "./"]
COPY ["discordbot.csproj", "./"]
RUN dotnet restore "discordbot.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "discordbot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "discordbot.csproj" -r linux-x64 -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS http://*:5000
ENTRYPOINT ["dotnet", "discordbot.dll"]