#!/bin/bash

NET_STANDARD="netstandard1.6"
NET_CORE_APP="netcoreapp1.1"
PACKAGE_OUTPUT="../../../../_packages"
NUGET_VERSION="2.0.30"

COMMAND="$1"

dotnet_restore_lib()
{

	files=("${!1}")

	for var in ${files[@]}
	do
		dotnet restore  "${var}" -r "$NET_STANDARD" -v q --no-dependencies
	done
}

dotnet_restore_app()
{
	files=("${!1}")

	for var in ${files[@]}
	do
		dotnet restore "${var}" -r "$NET_CORE_APP" -v q --no-dependencies
	done
}

# Sub routines for build different assembly types.
# --------------------------------------------------------

dotnet_build_lib()
{
	files=("${!1}")

	for var in ${files[@]}
	do
		echo "${var}"
		dotnet build  "${var}" -f "$NET_STANDARD" -v q --no-dependencies
	done
}

dotnet_build_app()
{
	files=("${!1}")

	for var in ${files[@]}
	do
		dotnet build "${var}" -f "$NET_CORE_APP" -v q --no-dependencies
	done
}

dotnet_build_root()
{
	files=("${!1}")

	for var in ${files[@]}
	do
		dotnet build "${var}" -f "$NET_CORE_APP" -v q
	done
}

dotnet_run_test()
{
	files=("${!1}")

	for var in ${files[@]}
	do
		dotnet test "${var}" -f "$NET_CORE_APP" -v q
	done
}

dotnet_pack()
{
	files=("${!1}")

	for var in ${files[@]}
	do
		dotnet pack "${var}" --no-build --include-symbols --output "$PACKAGE_OUTPUT" /p:TargetFrameworks=netstandard1.6 /p:VersionPrefix="$NUGET_VERSION"
	done
}


LIBRARIES=(
	./src/Common/NetFusion.Common/NetFusion.Common.csproj
	./src/Common/NetFusion.Base/NetFusion.Base.csproj
	./src/Core/NetFusion.Bootstrap/NetFusion.Bootstrap.csproj
	./src/Core/NetFusion.Test/NetFusion.Test.csproj

	./src/Domain/NetFusion.Domain/NetFusion.Domain.csproj
	./src/Domain/NetFusion.Domain.Messaging/NetFusion.Domain.Messaging.csproj

	./src/Utilities/NetFusion.Utilities.Validation/NetFusion.Utilities.Validation.csproj
	./src/Utilities/NetFusion.Utilities.Mapping/NetFusion.Utilities.Mapping.csproj

	./src/Core/NetFusion.Settings/NetFusion.Settings.csproj
	./src/Core/NetFusion.Logging/NetFusion.Logging.csproj
	./src/Core/NetFusion.Messaging/NetFusion.Messaging.csproj

	./src/Infrastructure/NetFusion.Web.Mvc/NetFusion.Web.Mvc.csproj
	./src/Infrastructure/NetFusion.EntityFramework/NetFusion.EntityFramework.csproj
	./src/Infrastructure/NetFusion.MongoDB/NetFusion.MongoDB.csproj
	./src/Infrastructure/NetFusion.RabbitMQ/NetFusion.RabbitMQ.csproj
	
	./src/Infrastructure/NetFusion.Rest.Client/NetFusion.Rest.Client.csproj
	./src/Infrastructure/NetFusion.Rest.Common/NetFusion.Rest.Common.csproj
	./src/Infrastructure/NetFusion.Rest.Config/NetFusion.Rest.Config.csproj
	./src/Infrastructure/NetFusion.Rest.Resource/NetFusion.Rest.Resource.csproj
	./src/Infrastructure/NetFusion.Rest.Server/NetFusion.Rest.Server.csproj

	./src/Integration/NetFusion.Logging.Serilog/NetFusion.Logging.Serilog.csproj
	./src/Integration/NetFusion.Domain.Roslyn/NetFusion.Domain.Roslyn.csproj
	./src/Integration/NetFusion.Domain.MongoDB/NetFusion.Domain.MongoDB.csproj
	./src/Integration/NetFusion.RabbitMQ.MongoDB/NetFusion.RabbitMQ.MongoDB.csproj
)

TESTS=(
	./test/CommonTests/CommonTests.csproj
	./test/CoreTests/CoreTests.csproj
	./test/InfrastructureTests/InfrastructureTests.csproj
	./test/IntegrationTests/IntegrationTests.csproj
	./test/UtilitiesTests/UtilitiesTests.csproj
)

APP_LIBS=(
	./samples/ExampleApi/ExampleApi.csproj
)

APPS=(
	./samples/ConsumerHost/ConsumerHost.csproj
	./samples/WebApiHost/WebApiHost.csproj
)

# Resores all projects.
if [ "$COMMAND" = "restore" ]; then
	dotnet_restore_lib LIBRARIES[@]
	dotnet_restore_app TESTS[@]
	dotnet_restore_lib APP_LIBS[@]
	dotnet_restore_app APPS[@]

elif [ "$COMMAND" = "build-all" ]; then
	dotnet_build_lib LIBRARIES[@]
	dotnet_build_app TESTS[@]
	dotnet_build_lib APP_LIBS[@]
	dotnet_build_app APPS[@]

elif [ "$COMMAND" = "build" ]; then
	dotnet_build_root TESTS[@]

elif [ "$COMMAND" = "build-samples" ]; then
	dotnet_build_lib APP_LIBS[@]
	dotnet_build_app APPS[@]

elif [ "$COMMAND" = "test" ]; then
	dotnet_run_test TESTS[@]

elif [ "$COMMAND" = "test-common" ]; then
	TESTS=(
		./test/CommonTests/CommonTests.csproj
	)
	dotnet_run_test TESTS[@]

elif [ "$COMMAND" = "test-core" ]; then
	TESTS=(
		./test/CoreTests/CoreTests.csproj
	)
	dotnet_run_test TESTS[@]

elif [ "$COMMAND" = "test-infrastructure" ]; then
	TESTS=(
		./test/InfrastructureTests/InfrastructureTests.csproj
	)
	dotnet_run_test TESTS[@]

elif [ "$COMMAND" = "test-integration" ]; then
	TESTS=(
		./test/IntegrationTests/IntegrationTests.csproj
	)
	dotnet_run_test TESTS[@]

elif [ "$COMMAND" = "pack" ]; then
	dotnet_pack LIBRARIES[@]
else
	echo "----------------------------------------------------------------"
	echo "-- Valid Commands:"
	echo "----------------------------------------------------------------"
	echo "-> restore: Restores all projects"
	echo "----------------------------------------------------------------"
	echo "-> build-all: Builds all source, tests, and samples"
	echo "-> build: Builds tests and changed referenced libraries"
	echo "-> build-samples: Builds samples"
	echo "----------------------------------------------------------------"
	echo "-> test: Builds changed referenced libraries and runs tests"
	echo "-> test-common: Builds and Tests common projects"
	echo "-> test-core: Builds and Tests core projects"
	echo "-> test-infrastructure: Builds and Tests core projects"
	echo "-> test-integration: Builds and Tests integration projects"
	echo "----------------------------------------------------------------"
	echo "-> pack: Generates all Nuget packages"
fi



