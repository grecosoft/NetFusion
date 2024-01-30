PACKAGES_DIR=../../../_packages
VERSION=8.0.0-dev

rm $PACKAGES_DIR/*.nupkg
dotnet clean
dotnet build NetFusion.sln

dotnet pack ./src/Common/NetFusion.Common/NetFusion.Common.csproj --configuration Debug --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Common/NetFusion.Common.Base/NetFusion.Common.Base.csproj --configuration Debug --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION

dotnet pack ./src/Core/NetFusion.Core.Bootstrap/NetFusion.Core.Bootstrap.csproj --configuration Debug --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Core/NetFusion.Core.Builder/NetFusion.Core.Builder.csproj --configuration Debug --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Core/NetFusion.Core.Settings/NetFusion.Core.Settings.csproj --configuration Debug --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Core/NetFusion.Core.TestFixtures/NetFusion.Core.TestFixtures.csproj --configuration Debug --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION

dotnet pack ./src/Messaging/NetFusion.Messaging/NetFusion.Messaging.csproj --configuration Debug --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Messaging/NetFusion.Messaging.Types/NetFusion.Messaging.Types.csproj --configuration Debug --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION

dotnet pack ./src/Services/NetFusion.Services.Mapping/NetFusion.Services.Mapping.csproj --configuration Debug --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Services/NetFusion.Services.Messaging/NetFusion.Services.Messaging.csproj --configuration Debug --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Services/NetFusion.Services.Roslyn/NetFusion.Services.Roslyn.csproj --configuration Debug --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Services/NetFusion.Services.Serialization/NetFusion.Services.Serialization.csproj --configuration Debug --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Services/NetFusion.Services.Serilog/NetFusion.Services.Serilog.csproj --configuration Debug --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION


dotnet pack ./src/Web/NetFusion.Web/NetFusion.Web.csproj --configuration Debug --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Web/NetFusion.Web.Common/NetFusion.Web.Common.csproj --configuration Debug --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Web/NetFusion.Web.Rest.Client/NetFusion.Web.Rest.Client.csproj --configuration Debug --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Web/NetFusion.Web.Rest.CodeGen/NetFusion.Web.Rest.CodeGen.csproj --configuration Debug --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Web/NetFusion.Web.Rest.Docs/NetFusion.Web.Rest.Docs.csproj --configuration Debug --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Web/NetFusion.Web.Rest.Resources/NetFusion.Web.Rest.Resources.csproj --configuration Debug --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Web/NetFusion.Web.Rest.Server/NetFusion.Web.Rest.Server.csproj --configuration Debug --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION

dotnet pack ./src/Integration/NetFusion.Integration.Bus/NetFusion.Integration.Bus.csproj --configuration Debug --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Integration/NetFusion.Integration.MongoDB/NetFusion.Integration.MongoDB.csproj --configuration Debug --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Integration/NetFusion.Integration.RabbitMQ/NetFusion.Integration.RabbitMQ.csproj --configuration Debug --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Integration/NetFusion.Integration.Redis/NetFusion.Integration.Redis.csproj --configuration Debug --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Integration/NetFusion.Integration.ServiceBus/NetFusion.Integration.ServiceBus.csproj --configuration Debug --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION



