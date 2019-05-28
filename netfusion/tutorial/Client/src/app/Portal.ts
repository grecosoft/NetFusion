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
        menuDefinition.defineArea(new ApplicationArea("overview", "Overview", "topic", [
            new AreaMenuItem("tutorial", "Tutorial", "applications", "overview/tutorial"),
            new AreaMenuItem("resources", "Resources", "applications", "overview/resources")
        ]));

        menuDefinition.defineArea(new ApplicationArea("bootstrapping", "Bootstrapping", "topic", [
            new AreaMenuItem("composite-application", "Composite App", "applications", "areas/core/bootstrapping/composite-application"),
            new AreaMenuItem("composite-builder", "Composite Builder", "applications", "areas/core/bootstrapping/composite-builder"),
            new AreaMenuItem("composite-log", "Composite Log", "applications", "areas/core/bootstrapping/composite-log"),
        ]));

        menuDefinition.defineArea(new ApplicationArea("baseImp", "Base", "topic", [
            new AreaMenuItem("validation", "Validation", "applications", "areas/common/validation"),
            new AreaMenuItem("mapping", "Mapping", "applications", "areas/common/mapping"),
            new AreaMenuItem("attrib-entity", "Attributed Entity", "applications", "areas/common/attrib-entity"),
            new AreaMenuItem("roslyn-expressions", "Roslyn Expressions", "applications", "areas/common/entity-exp")
        ]));

        menuDefinition.defineArea(new ApplicationArea("settings", "Settings", "plugin", [
            new AreaMenuItem("settings-example", "Examples", "applications", "areas/core/settings"),
        ]));

        menuDefinition.defineArea(new ApplicationArea("messaging", "Messaging", "plugin", [
            new AreaMenuItem("commands", "Commands", "applications", "areas/core/messaging/commands"),
            new AreaMenuItem("domain-events", "Domain Events", "applications", "areas/core/messaging/domain-events"),
            new AreaMenuItem("queries", "Queries", "applications", "areas/core/messaging/queries"),
            new AreaMenuItem("enrichers", "Enrichers", "applications", "areas/core/messaging/enrichers"),
            new AreaMenuItem("rules", "Rules", "applications", "areas/core/messaging/rules"),
            new AreaMenuItem("publishers", "Publishers", "applications", "areas/core/messaging/publishers")
        ]));

        menuDefinition.defineArea(new ApplicationArea("mongodb", "MongoDB", "plugin", [
            new AreaMenuItem("mongodb", "Examples", "applications", "areas/integration/database/mongodb"),
        ]));

        menuDefinition.defineArea(new ApplicationArea("entityframework", "Entity Framework", "plugin", [
            new AreaMenuItem("entityframework", "Examples", "applications", "areas/integration/database/entityframework"),
        ]));

        menuDefinition.defineArea(new ApplicationArea("rabbitmq", "RabbitMQ", "plugin", [
            new AreaMenuItem("rabbitmq-direct", "Direct", "applications", "areas/integration/rabbitmq/direct"),
            new AreaMenuItem("rabbitmq-topic", "Topic", "applications", "areas/integration/rabbitmq/topic"),
            new AreaMenuItem("rabbitmq-fanout", "Fanout", "applications", "areas/integration/rabbitmq/fanout"),
            new AreaMenuItem("rabbitmq-queue", "Queue", "applications", "areas/integration/rabbitmq/queue"),
            new AreaMenuItem("rabbitmq-rpc", "Rpc", "applications", "areas/integration/rabbitmq/rpc")
        ]));

        menuDefinition.defineArea(new ApplicationArea("qmqp", "AMQP", "plugin", [
            new AreaMenuItem("amqp-publisher", "Publisher", "applications", "areas/integration/amqp/publisher"),
            new AreaMenuItem("amqp-consumer", "Consumer", "applications", "areas/integration/amqp/consumer")
        ]));

        menuDefinition.defineArea(new ApplicationArea("redis", "Redis", "plugin", [
            new AreaMenuItem("redis-data", "Data", "applications", "areas/integration/redis/data"),
            new AreaMenuItem("redis-channels", "Pub/Sub", "applications",    "areas/integration/redis/channels")
        ]));

        menuDefinition.defineArea(new ApplicationArea("rest", "REST", "plugin", [
            new AreaMenuItem("resource-linking", "Resource Linking", "applications", "areas/web/rest/resource-linking"),
            new AreaMenuItem("template-urls", "Template Urls", "applications", "areas/web/rest/template-urls")
        ]));
        
        
        menuDefinition.setDefaultAreaMenuKey("overview", "tutorial-info");
    }
}
