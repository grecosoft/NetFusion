mkdir libs
xcopy /s/y ..\..\src\NetFusion\NetFusion.Common\bin\debug\NetFusion.Common.dll .\libs\
xcopy /s/y ..\..\src\NetFusion\NetFusion.Common\bin\debug\Newtonsoft.Json.dll .\libs\
xcopy /s/y ..\..\src\NetFusion\NetFusion.Bootstrap\bin\debug\NetFusion.Bootstrap.dll .\libs\
xcopy /s/y ..\..\src\NetFusion\NetFusion.Bootstrap\bin\debug\AutoFac.dll .\libs\
xcopy /s/y ..\..\src\NetFusion\NetFusion.Settings\bin\debug\NetFusion.Settings.dll .\libs\
xcopy /s/y ..\..\src\NetFusion\NetFusion.Domain\bin\debug\NetFusion.Domain.dll .\libs\
xcopy /s/y ..\..\src\NetFusion\NetFusion.Messaging\bin\debug\NetFusion.Messaging.dll .\libs\
xcopy /s/y ..\..\src\NetFusion\NetFusion.MongoDB\bin\debug\NetFusion.MongoDB.dll .\libs\

xcopy /s/y ..\..\src\NetFusion\NetFusion.MongoDB\bin\debug\MongoDB.Bson.dll .\libs\
xcopy /s/y ..\..\src\NetFusion\NetFusion.MongoDB\bin\debug\MongoDB.Driver.Core.dll .\libs\
xcopy /s/y ..\..\src\NetFusion\NetFusion.MongoDB\bin\debug\MongoDB.Driver.dll .\libs\
xcopy /s/y ..\..\src\NetFusion\NetFusion.EntityFramework\bin\debug\NetFusion.EntityFramework.dll .\libs\

xcopy /s/y ..\..\src\NetFusion\NetFusion.RabbitMQ\bin\debug\NetFusion.RabbitMQ.dll .\libs\
xcopy /s/y ..\..\src\NetFusion\NetFusion.RabbitMQ\bin\debug\RabbitMQ.Client.dll .\libs\

xcopy /s/y ..\..\src\NetFusion\NetFusion.Integration\bin\debug\NetFusion.Integration.dll .\libs\
xcopy /s/y ..\..\src\NetFusion\NetFusion.Logging.Serilog\bin\debug\NetFusion.Logging.Serilog.dll .\libs\
xcopy /s/y ..\..\src\NetFusion\NetFusion.Domain.Roslyn\bin\debug\NetFusion.Domain.Roslyn.dll .\libs\
xcopy /s/y ..\..\src\NetFusion\NetFusion.Settings.MongoDB\bin\debug\NetFusion.Settings.MongoDb.dll .\libs\