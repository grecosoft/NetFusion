#!/bin/bash

# ./Build.NetFusion-core.sh -v=7.0.0 -c=test -o=/home/greco/_dev/git/_packages

# -------------------------------------
# Command Line Parameters
# -------------------------------------
NUGET_VERSION=""
LOCAL_PACKAGE_DIR=""

# -------------------------------------
# Build Tasks
# -------------------------------------
dotnet_restore()
{
	dotnet restore "$1" 
}

dotnet_build()
{
	dotnet build "$1" --framework netcoreapp2.1 --no-restore
}

dotnet_test()
{
	dotnet test "$1" --framework netcoreapp2.1 --no-build
}

dotnet_pack()
{
	dotnet pack "$1" --output "$LOCAL_PACKAGE_DIR" --no-build /p:TargetFrameworks=netstandard2.0 /p:VersionPrefix="$NUGET_VERSION"
}

# -------------------------------------
# Common Related Projects
# -------------------------------------
build_common()
{
	dotnet_restore ./test/CommonTests/CommonTests.csproj
	dotnet_build ./test/CommonTests/CommonTests.csproj
}

test_common()
{
	dotnet_test ./test/CommonTests/CommonTests.csproj
}

# -------------------------------------
# Core Related Projects
# -------------------------------------
build_core()
{
	dotnet_restore ./test/CoreTests/CoreTests.csproj
	dotnet_build ./test/CoreTests/CoreTests.csproj
}

test_core()
{
	dotnet_test ./test/CoreTests/CoreTests.csproj
}

# -------------------------------------
# Package Generation
# -------------------------------------
generate_packages()
{
	# Generate nuget packages:
	dotnet_pack ./src/Common/NetFusion.Common/NetFusion.Common.csproj
	dotnet_pack ./src/Common/NetFusion.Base/NetFusion.Base.csproj
	dotnet_pack ./src/Common/NetFusion.Mapping/NetFusion.Mapping.csproj
	dotnet_pack ./src/Common/NetFusion.Roslyn/NetFusion.Roslyn.csproj
	dotnet_pack ./src/Core/NetFusion.Bootstrap/NetFusion.Bootstrap.csproj
	dotnet_pack ./src/Core/NetFusion.Settings/NetFusion.Settings.csproj
	dotnet_pack ./src/Core/NetFusion.Messaging.Types/NetFusion.Messaging.Types.csproj
	dotnet_pack ./src/Core/NetFusion.Messaging/NetFusion.Messaging.csproj
	dotnet_pack ./src/Core/NetFusion.Test/NetFusion.Test.csproj
}

# -------------------------------------
# Script Execution
# -------------------------------------
for i in "$@"
do
case $i in
    -v=*|--version=*)
    NUGET_VERSION="${i#*=}"
    shift # past argument=value
    ;;
		-c=*|--command=*)
    COMMAND="${i#*=}"
    shift # past argument=value
    ;;
		-o=*|--output=*)
    LOCAL_PACKAGE_DIR="${i#*=}"
    shift # past argument=value
    ;;
    *)
          # unknown option
    ;;
esac
done

if [ "$NUGET_VERSION" == "" ] || [ "$LOCAL_PACKAGE_DIR" == "" ]; then
	echo "-v/--version and -o/--output parameters not specified."	
	exit 1
fi

if [ "$COMMAND" = "build" ]; then
	build_common
	build_core
  generate_packages
	
elif [ "$COMMAND" = "test" ]; then
	build_common
	build_core
	test_common
	test_core
	generate_packages

else
	echo "----------------------------------------------------------------"
	echo "-- Commands:"
	echo "----------------------------------------------------------------"
	echo "-> build: Restores, builds, and generates Nuget Packages"
	echo "-> test: Restores, builds, tests, and generates Nuget Packages"
fi








