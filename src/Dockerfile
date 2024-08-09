#docker run -d -e ConnectionStrings__DefaultConnection="Data Source=motofact.db" -e JAEGER_SERVICE_NAME=MotoFacts -p 8088:80 motofactapi
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY MotoFacts.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
EXPOSE 80
COPY --from=build /app .

ENTRYPOINT ["dotnet", "MotoFacts.dll"]