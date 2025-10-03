# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the csproj and restore dependencies
COPY ReimbursementProject/ReimbursementProject.csproj ./ReimbursementProject/
RUN dotnet restore ReimbursementProject/ReimbursementProject.csproj

# Copy everything and build
COPY . .
WORKDIR /src/ReimbursementProject
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Serve the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "ReimbursementProject.dll"]
