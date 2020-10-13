FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY . .
RUN dotnet restore "discordbot.csproj"
WORKDIR "/src"
RUN dotnet build "discordbot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "discordbot.csproj" -r linux-x64 -c Release -o /app/publish

FROM base AS final
WORKDIR /publish
COPY --from=publish /app/publish /publish
CMD tail -f /dev/null
