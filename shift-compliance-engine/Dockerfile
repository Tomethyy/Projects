FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ShiftEngine.sln ./
COPY src/ShiftEngine.Domain/ShiftEngine.Domain.csproj src/ShiftEngine.Domain/
COPY src/ShiftEngine.Application/ShiftEngine.Application.csproj src/ShiftEngine.Application/
COPY src/ShiftEngine.Infrastructure/ShiftEngine.Infrastructure.csproj src/ShiftEngine.Infrastructure/
COPY src/ShiftEngine.Compliance.ArbZG/ShiftEngine.Compliance.ArbZG.csproj src/ShiftEngine.Compliance.ArbZG/
COPY src/ShiftEngine.Replanning/ShiftEngine.Replanning.csproj src/ShiftEngine.Replanning/
COPY src/ShiftEngine.Api/ShiftEngine.Api.csproj src/ShiftEngine.Api/
COPY NuGet.Config ./
RUN dotnet restore src/ShiftEngine.Api/ShiftEngine.Api.csproj
COPY src/ src/
RUN dotnet publish src/ShiftEngine.Api/ShiftEngine.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "ShiftEngine.Api.dll"]
