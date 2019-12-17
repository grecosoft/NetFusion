FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /build/src

# --------------------------------------
# RESTORE
# --------------------------------------

# Copy microservice components:
COPY ./src/Components/Solution.Context.App/*.csproj ./Components/Solution.Context.App/
COPY ./src/Components/Solution.Context.Domain/*.csproj ./Components/Solution.Context.Domain/
COPY ./src/Components/Solution.Context.Infra/*.csproj ./Components/Solution.Context.Infra/

# Copy microservice hosts:
COPY ./src/Solution.Context.Tests/*.csproj ./Solution.Context.Tests/
COPY ./src/Solution.Context.WebApi/*.csproj ./Solution.Context.WebApi/

# Copy the solution file to restore all projects:
COPY ./Solution.Context.sln /build
RUN dotnet restore /build

# --------------------------------------
# BUILD / TEST / PUBLISH
# --------------------------------------

# Copy all of the source to be built:
COPY ./src ./
RUN dotnet build /build 
RUN dotnet test ./Solution.Context.Tests/ --no-build
RUN dotnet publish ./Solution.Context.WebApi/Solution.Context.WebApi.csproj --output ../../out --no-build

# --------------------------------------
# CREATE IMAGE FOR CONTAINER CREATION
# --------------------------------------
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1	
WORKDIR /microservice

COPY --from=build-env /build/out ./
ENTRYPOINT ["dotnet", "Solution.Context.WebApi.dll"]
EXPOSE 80