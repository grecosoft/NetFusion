PACKAGES_DIR=../../_packages
VERSION=9.9.40

dotnet clean
dotnet build NetFusion.sln

dotnet pack ./src/Common/NetFusion.Common/NetFusion.Common.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Common/NetFusion.Base/NetFusion.Base.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Common/NetFusion.Mapping/NetFusion.Mapping.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION

dotnet pack ./src/Core/NetFusion.Bootstrap/NetFusion.Bootstrap.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Core/NetFusion.Settings/NetFusion.Settings.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Core/NetFusion.Messaging.Types/NetFusion.Messaging.Types.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Core/NetFusion.Messaging/NetFusion.Messaging.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Core/NetFusion.Builder/NetFusion.Builder.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Core/NetFusion.Test/NetFusion.Test.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION

dotnet pack ./src/Integration/NetFusion.Serialization/NetFusion.Serialization.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Integration/NetFusion.Roslyn/NetFusion.Roslyn.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Integration/NetFusion.EntityFramework/NetFusion.EntityFramework.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Integration/NetFusion.MongoDB/NetFusion.MongoDB.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Integration/NetFusion.Redis/NetFusion.Redis.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Integration/NetFusion.AMQP/NetFusion.AMQP.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Integration/NetFusion.RabbitMq/NetFusion.RabbitMQ.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION

dotnet pack ./src/Web/NetFusion.Web.Mvc/NetFusion.Web.Mvc.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Web/NetFusion.Rest.Common/NetFusion.Rest.Common.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Web/NetFusion.Rest.Resources/NetFusion.Rest.Resources.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Web/NetFusion.Rest.Server/NetFusion.Rest.Server.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Web/NetFusion.Rest.Client/NetFusion.Rest.Client.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Web/NetFusion.Rest.Docs/NetFusion.Rest.Docs.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION
dotnet pack ./src/Web/NetFusion.Rest.CodeGen/NetFusion.Rest.CodeGen.csproj --no-build --output $PACKAGES_DIR -p:PackageVersion=$VERSION

