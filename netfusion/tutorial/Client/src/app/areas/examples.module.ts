import { NgModule } from '@angular/core';
import { SettingsComponent } from './core/settings/settings.component/settings.component';
import { AreaCommonModule } from './area-common.module';
import { Routes, RouterModule } from '@angular/router';
import { SettingsService } from './core/settings/SettingsService';
import { MongoDbComponent } from './integration/mongoDb/mongoDb.component/mongoDb.component';
import { MongoDbService } from './integration/mongoDb/MongoDbService';
import { JsonPipe } from '@angular/common';
import { RabbitMqDirectComponent } from './integration/rabbitMq/rabbitMq-direct.component/rabbitMq-direct.component';
import { RabbitMqTopicComponent } from './integration/rabbitMq/rabbitMq-topic.component/rabbitMq-topic.component';
import { RabbitMqFanoutComponent } from './integration/rabbitMq/rabbitMq-fanout.component/rabbitMq-fanout.component';
import { RabbitMqQueueComponent } from './integration/rabbitMq/rabbitMq-queue.component/rabbitMq-queue.component';
import { RabbitMqRpcComponent } from './integration/rabbitMq/rabbitMq-rpc.component/rabbitMq-rpc.component';
import { RabbitMqService } from './integration/rabbitMq/RabbitMqService';
import { GitHubService } from './github/GitHubService';
import { CodeViewerComponent } from './github/code-viewer/code-viewer.component';
import { RedisDataComponent } from './integration/redis/redis-data.component/redis-data.component';
import { RedisService } from './integration/redis/RedisService';
import { CompositeLogComponent } from './core/bootstrapping/composite-log/composite-log.component';
import { OverviewService } from './overview/OverviewService';
import { RedisChannelsComponent } from './integration/redis/redis-channels.component/redis-channels.component';
import { AttributedEntityComponent } from './common/attributed-entity.component/attributed-entity.component';
import { EntityExpressionsComponent } from './common/entity-expressions.component/entity-expressions.component';
import { AmqpPublisherComponent } from './integration/amqp/amqp-publisher.component/amqp-publisher.component';
import { AmqpConsumerComponent } from './integration/amqp/amqp-consumer.component/amqp-consumer.component';
import { AmqpService } from './integration/amqp/AmqpService';
import { CommandsComponent } from './core/messaging/commands.component/commands.component';
import { DomainEventsComponent } from './core/messaging/domain-events.component/domain-events.component';
import { QueriesComponent } from './core/messaging/queries.component/queries.component';
import { EnrichersComponent } from './core/messaging/enrichers/enrichers.component';
import { MessagingService } from './core/messaging/MessagingService';
import { TutorialInfoComponent } from './overview/tutorial-info/tutorial-info.component';
import { RulesComponent } from './core/messaging/rules/rules.component';
import { PublishersComponent } from './core/messaging/publishers/publisher.component';
import { CompositeBuilderComponent } from './core/bootstrapping/composite-builder/composite-builder.component';
import { CompositeApplicationComponent } from './core/bootstrapping/composite-application/composite-application.component';
import { MappingComponent } from './common/mapping-component/mapping.component';
import { ValidationComponent } from './common/validation.component/validation.component';

const areaRoutes: Routes = [
    { path: 'overview/tutorial-info', component: TutorialInfoComponent },

    { path: 'settings', component: SettingsComponent },
    { path: 'mongodb', component: MongoDbComponent },

    { path: 'bootstrapping/composite-application', component: CompositeApplicationComponent },
    { path: 'bootstrapping/composite-builder', component: CompositeBuilderComponent },
    { path: 'bootstrapping/composite-log', component: CompositeLogComponent },

    { path: 'base/attrib-entity', component: AttributedEntityComponent },
    { path: 'base/entity-exp', component: EntityExpressionsComponent },
    { path: 'base/mapping', component: MappingComponent },
    { path: 'base/validation', component: ValidationComponent },

    { path: 'messaging/commands', component: CommandsComponent },
    { path: 'messaging/domain-events', component: DomainEventsComponent },
    { path: 'messaging/queries', component: QueriesComponent },
    { path: 'messaging/enrichers', component: EnrichersComponent },
    { path: 'messaging/rules', component: EnrichersComponent },
    { path: 'messaging/publishers', component: EnrichersComponent },

    { path: 'rabbitmq/direct', component: RabbitMqDirectComponent },
    { path: 'rabbitmq/topic', component: RabbitMqTopicComponent },
    { path: 'rabbitmq/fanout', component: RabbitMqFanoutComponent },
    { path: 'rabbitmq/queue', component: RabbitMqQueueComponent },
    { path: 'rabbitmq/rpc', component: RabbitMqRpcComponent },

    { path: 'amqp/publisher', component: AmqpPublisherComponent },
    { path: 'amqp/consumer', component: AmqpConsumerComponent },

    { path: 'redis/data', component: RedisDataComponent },
    { path: 'redis/channels', component: RedisChannelsComponent },
  ];

@NgModule({
    imports: [
        AreaCommonModule,
        RouterModule.forChild(
            areaRoutes
          )
    ],
    declarations: [
        TutorialInfoComponent,
        CompositeLogComponent,

        CodeViewerComponent,

        CompositeBuilderComponent,
        CompositeApplicationComponent,
        MappingComponent,
        ValidationComponent,

        AttributedEntityComponent,
        EntityExpressionsComponent,
        RulesComponent,

        CommandsComponent,
        DomainEventsComponent,
        QueriesComponent,
        EnrichersComponent,

        SettingsComponent,
        MongoDbComponent,

        RabbitMqDirectComponent,
        RabbitMqTopicComponent,
        RabbitMqFanoutComponent,
        RabbitMqQueueComponent,
        RabbitMqRpcComponent,
        PublishersComponent,

        RedisDataComponent,
        RedisChannelsComponent,

        AmqpPublisherComponent,
        AmqpConsumerComponent
    ],
    entryComponents: [
        CompositeLogComponent,
        SettingsComponent,
        MongoDbComponent,

        RulesComponent,

        TutorialInfoComponent,
        MappingComponent,
        ValidationComponent,

        AttributedEntityComponent,
        EntityExpressionsComponent,

        CommandsComponent,
        DomainEventsComponent,
        QueriesComponent,
        EnrichersComponent,
        CompositeBuilderComponent,
        CompositeApplicationComponent,
        PublishersComponent,

        RabbitMqDirectComponent,
        RabbitMqTopicComponent,
        RabbitMqFanoutComponent,
        RabbitMqQueueComponent,
        RabbitMqRpcComponent,

        RedisDataComponent,
        RedisChannelsComponent,

        AmqpPublisherComponent,
        AmqpConsumerComponent
    ],
    providers: [
        JsonPipe,
        SettingsService,
        MongoDbService,
        RabbitMqService,
        AmqpService,
        GitHubService,
        RedisService,
        OverviewService,
        MessagingService
    ]
})
export class ExamplesModule {

}