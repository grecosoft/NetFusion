FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build-env
WORKDIR /build/src

# --------------------------------------
# RESTORE
# --------------------------------------

# Copy microservice components:
COPY ./src/Components/Demo.App/*.csproj ./Components/Demo.App/
COPY ./src/Components/Demo.Domain/*.csproj ./Components/Demo.Domain/
COPY ./src/Components/Demo.Infra/*.csproj ./Components/Demo.Infra/

# Copy microservice hosts:
COPY ./src/Demo.Tests/*.csproj ./Demo.Tests/
COPY ./src/Demo.WebApi/*.csproj ./Demo.WebApi/

# Copy the solution file to restore all projects:
COPY ./Demo.sln /build
RUN dotnet restore /build

# --------------------------------------
# BUILD / TEST / PUBLISH
# --------------------------------------

# Copy all of the source to be built:
COPY ./src ./
RUN dotnet build /build 
RUN dotnet test ./Demo.Tests/ --no-build
RUN dotnet publish ./Demo.WebApi/Demo.WebApi.csproj --output ../../out --no-build

# --------------------------------------
# CREATE IMAGE FOR CONTAINER CREATION
# --------------------------------------
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2	
WORKDIR /microservice

COPY --from=build-env /build/out ./
ENTRYPOINT ["dotnet", "Demo.WebApi.dll"]
EXPOSE 80