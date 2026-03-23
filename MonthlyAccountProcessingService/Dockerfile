FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY MonthlyAccountProcessingService/MonthlyAccountProcessingService.csproj MonthlyAccountProcessingService/
COPY AccountsStatementsDataSolution/AccountsStatementsData/AccountsStatementsData.csproj AccountsStatementsDataSolution/AccountsStatementsData/
RUN dotnet restore MonthlyAccountProcessingService/MonthlyAccountProcessingService.csproj

# Copy everything else and build
COPY MonthlyAccountProcessingService/ MonthlyAccountProcessingService/
COPY AccountsStatementsDataSolution/AccountsStatementsData/ AccountsStatementsDataSolution/AccountsStatementsData/
RUN dotnet publish MonthlyAccountProcessingService/MonthlyAccountProcessingService.csproj -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "MonthlyAccountProcessingService.dll"]
