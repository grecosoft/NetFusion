FROM  mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
WORKDIR /build/src

LABEL "net-fusion.template.layer"="Contains Restored NuGets"
LABEL "net-fusion.project.url"="https://github.com/grecosoft/NetFusion"

# --------------------------------------------------------------------------------------
# RESTORE [Stage 1]
# --------------------------------------------------------------------------------------
# - This section copies only the solution and projects files used to restore the needed 
#   NuGet packages.  The restored packages are placed in:  /root/.nuget/packages.
#
# - The reason for only copying the solution and project files is so the created layers
#   will be smaller (a restore does not require the source.)  When the next stage is
#   is executed (Build/Test/Publish) for the first time or all subsequent builds,
#   these layers will already exist, and if not changed, will not have to be built.
#
# - These layers would change, for example, if a new Nuget package was added to any
#   of the projects. 
#
# - Since the source of the microservice will change the most often, these layers will
#   be reused and will not have be recreated when the updated source is built. 
# --------------------------------------------------------------------------------------

# Copy microservice components:
COPY ./src/Components/Solution.Context.App/*.csproj ./Components/Solution.Context.App/
COPY ./src/Components/Solution.Context.Domain/*.csproj ./Components/Solution.Context.Domain/
COPY ./src/Components/Solution.Context.Infra/*.csproj ./Components/Solution.Context.Infra/

# Copy microservice hosts:
COPY ./src/Solution.Context.Tests/*.csproj ./Solution.Context.Tests/
COPY ./src/Solution.Context.WebApi/*.csproj ./Solution.Context.WebApi/

# Copy the solution file to restore all projects:
COPY ./src/Solution.Context.sln ./
RUN dotnet restore 
RUN rm -rd /build/src

# # --------------------------------------------------------------------------------------
# # BUILD / TEST / PUBLISH [Stage 2]
# # --------------------------------------------------------------------------------------

# Copy all the source and build the microservice.
COPY ./src ./
RUN dotnet build ./Solution.Context.sln

RUN dotnet test ./Solution.Context.Tests/ --no-build
RUN dotnet publish ./Solution.Context.WebApi/Solution.Context.WebApi.csproj --output ../out --no-build

# --------------------------------------------------------------------------------------
# CREATE IMAGE FOR CONTAINER CREATION [State 3]
# - This state takes the published output from [State 2] and copies them in to a layer
#   belonging to a new Docker image based on the aspnet core base image.  
# - This base image is much smaller since it is a non SDK runtime for asp.net core.
# --------------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /microservice

COPY --from=build-env /build/out ./
ENTRYPOINT ["dotnet", "Solution.Context.WebApi.dll"]
EXPOSE 80