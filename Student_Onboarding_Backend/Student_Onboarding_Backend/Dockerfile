# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Student Onboarding Platform/Student Onboarding Platform.csproj", "Student Onboarding Platform/"]
RUN dotnet restore "Student Onboarding Platform/Student Onboarding Platform.csproj"

COPY ["Student Onboarding Platform/", "Student Onboarding Platform/"]
RUN dotnet publish "Student Onboarding Platform/Student Onboarding Platform.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080
ENTRYPOINT ["dotnet", "Student Onboarding Platform.dll"]