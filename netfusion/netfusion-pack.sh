PACKAGES_DIR=../../../_packages
VERSION=7.0.10-dev

rm $PACKAGES_DIR/*.nupkg
dotnet clean
dotnet build NetFusion.sln

dotnet pack ./src/Common/NetFusion.Common/NetFusion.Common.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Common/NetFusion.Common.Base/NetFusion.Common.Base.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION

dotnet pack ./src/Core/NetFusion.Core.Bootstrap/NetFusion.Core.Bootstrap.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Core/NetFusion.Core.Builder/NetFusion.Core.Builder.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Core/NetFusion.Core.Settings/NetFusion.Core.Settings.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Core/NetFusion.Core.TestFixtures/NetFusion.Core.TestFixtures.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION

dotnet pack ./src/Messaging/NetFusion.Messaging/NetFusion.Messaging.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Messaging/NetFusion.Messaging.Types/NetFusion.Messaging.Types.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION

dotnet pack ./src/Services/NetFusion.Services.Mapping/NetFusion.Services.Mapping.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Services/NetFusion.Services.Messaging/NetFusion.Services.Messaging.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Services/NetFusion.Services.Roslyn/NetFusion.Services.Roslyn.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Services/NetFusion.Services.Serialization/NetFusion.Services.Serialization.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Services/NetFusion.Services.Serilog/NetFusion.Services.Serilog.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION


dotnet pack ./src/Web/NetFusion.Web/NetFusion.Web.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Web/NetFusion.Web.Common/NetFusion.Web.Common.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Web/NetFusion.Web.Rest.Client/NetFusion.Web.Rest.Client.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Web/NetFusion.Web.Rest.CodeGen/NetFusion.Web.Rest.CodeGen.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Web/NetFusion.Web.Rest.Docs/NetFusion.Web.Rest.Docs.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Web/NetFusion.Web.Rest.Resources/NetFusion.Web.Rest.Resources.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Web/NetFusion.Web.Rest.Server/NetFusion.Web.Rest.Server.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION

dotnet pack ./src/Integration/NetFusion.Integration.Bus/NetFusion.Integration.Bus.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Integration/NetFusion.Integration.MongoDB/NetFusion.Integration.MongoDB.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Integration/NetFusion.Integration.RabbitMQ/NetFusion.Integration.RabbitMQ.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Integration/NetFusion.Integration.Redis/NetFusion.Integration.Redis.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Integration/NetFusion.Integration.ServiceBus/NetFusion.Integration.ServiceBus.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION



