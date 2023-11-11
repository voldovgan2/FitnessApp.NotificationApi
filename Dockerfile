FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
RUN mkdir FitnessApp.IdentityServer
COPY FitnessApp.IdentityServer ./FitnessApp.IdentityServer
WORKDIR /src/FitnessApp.IdentityServer

RUN dotnet restore "FitnessApp.IdentityServer.csproj"
RUN dotnet build "FitnessApp.IdentityServer.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "FitnessApp.IdentityServer.csproj" -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app
COPY --from=publish /app .
EXPOSE 80 443
ENTRYPOINT ["dotnet", "FitnessApp.IdentityServer.dll"]