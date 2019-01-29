import { Observable } from 'rxjs';

import { ApplicationArea, AreaMenuItem } from './common/navigation/navigation.models';
import { Application } from './Application';
import { MenuDefinitionService } from './common/navigation/services/MenuDefinitionService';


// Class called when the ApplicationModule bootstraps.  This class configures the
// navigation and bootstraps the application instance.
export class Portal {

    public static bootstrap(
        application: Application,
        menuDefinition: MenuDefinitionService) : Observable<any> {
        
        this.defineAreas(menuDefinition);

        return application.bootstrap();
    }

    public static defineAreas(menuDefinition: MenuDefinitionService) {
        menuDefinition.defineArea(new ApplicationArea("overview", "Overview", "device", [
            new AreaMenuItem("overview-composite-log", "Composite Log", "applications", "areas/core/overview/composite-log"),
        ]));

        menuDefinition.defineArea(new ApplicationArea("settings", "NetFusion.Settings", "device", [
            new AreaMenuItem("settings-example", "Examples", "applications", "areas/core/settings"),
        ]));

        menuDefinition.defineArea(new ApplicationArea("mongodb", "NetFusion.MongoDB", "device", [
            new AreaMenuItem("mongodb-example", "Examples", "applications", "areas/core/mongodb"),
        ]));

        menuDefinition.defineArea(new ApplicationArea("rabbitmq", "NetFusion.RabbitMQ", "device", [
            new AreaMenuItem("rabbitmq-direct", "Direct", "applications", "areas/core/rabbitmq/direct"),
            new AreaMenuItem("rabbitmq-topic", "Topic", "applications", "areas/core/rabbitmq/topic"),
            new AreaMenuItem("rabbitmq-fanout", "Fanout", "applications", "areas/core/rabbitmq/fanout"),
            new AreaMenuItem("rabbitmq-queue", "Queue", "applications", "areas/core/rabbitmq/queue"),
            new AreaMenuItem("rabbitmq-rpc", "Rpc", "applications", "areas/core/rabbitmq/rpc")
        ]));

        menuDefinition.defineArea(new ApplicationArea("redis", "NetFusion.Redis", "device", [
            new AreaMenuItem("redis-data", "Data", "applications", "areas/core/redis/data"),
            new AreaMenuItem("redis-channels", "Pub/Sub", "applications", "areas/core/redis/channels")
        ]));

        // menuDefinition.defineArea(new ApplicationArea("devices", "NetFusion.Messaging", "device", [
        //     new AreaMenuItem("applications", "Commands", "applications", "areas/devices/applications"),
		//     new AreaMenuItem("applications", "Domain Events", "applications", "areas/devices/applications"),
		// 	new AreaMenuItem("applications", "Queries", "applications", "areas/devices/applications")
        // ]));  

        // menuDefinition.defineArea(new ApplicationArea("devices", "NetFusion.Mapping", "device", [
        //     new AreaMenuItem("applications", "Applications", "applications", "areas/devices/applications"),
        // ]));

        // menuDefinition.defineArea(new ApplicationArea("devices", "NetFusion.Base", "device", [
        //     new AreaMenuItem("applications", "Attributed Entity", "applications", "areas/devices/applications"),
		// 	    new AreaMenuItem("applications", "Validation", "applications", "areas/devices/applications")
        // ]));

        // menuDefinition.defineArea(new ApplicationArea("devices", "NetFusion.Rolsyn", "device", [
        //     new AreaMenuItem("applications", "Applications", "applications", "areas/devices/applications"),
        // ]));

        // menuDefinition.defineArea(new ApplicationArea("devices", "NetFusion.Azure.Messaging", "device", [
        //     new AreaMenuItem("applications", "Applications", "applications", "areas/devices/applications"),
        // ]));

        // menuDefinition.defineArea(new ApplicationArea("devices", "NetFusion.Rest", "device", [
        //     new AreaMenuItem("applications", "Applications", "applications", "areas/devices/applications"),
        // ]));

		// menuDefinition.defineArea(new ApplicationArea("devices", "NetFusion.EntityFramework", "device", [
        //     new AreaMenuItem("applications", "Applications", "applications", "areas/devices/applications"),
        // ]));
        
        menuDefinition.setDefaultAreaMenuKey("settings", "settings-example");
    }
}