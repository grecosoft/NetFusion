import { NgModule } from '@angular/core';
import { AmqpConsumerComponent } from './amqp/amqp-consumer.component/amqp-consumer.component';
import { AmqpPublisherComponent } from './amqp/amqp-publisher.component/amqp-publisher.component';
import { RedisChannelsComponent } from './redis/redis-channels.component/redis-channels.component';
import { RedisDataComponent } from './redis/redis-data.component/redis-data.component';
import { PublishersComponent } from '../core/messaging/publishers/publisher.component';
import { RabbitMqRpcComponent } from './rabbitMq/rabbitMq-rpc.component/rabbitMq-rpc.component';
import { RabbitMqQueueComponent } from './rabbitMq/rabbitMq-queue.component/rabbitMq-queue.component';
import { RabbitMqFanoutComponent } from './rabbitMq/rabbitMq-fanout.component/rabbitMq-fanout.component';
import { RabbitMqTopicComponent } from './rabbitMq/rabbitMq-topic.component/rabbitMq-topic.component';
import { RabbitMqDirectComponent } from './rabbitMq/rabbitMq-direct.component/rabbitMq-direct.component';
import { MongoDbComponent } from './mongoDb/mongoDb.component/mongoDb.component';
import { MongoDbService } from './mongoDb/MongoDbService';
import { RabbitMqService } from './rabbitMq/RabbitMqService';
import { AmqpService } from './amqp/AmqpService';
import { RedisService } from './redis/RedisService';
import { Routes, RouterModule } from '@angular/router';
import { AreaCommonModule } from '../area-common.module';
import { CodeViewerComponent } from '../github/code-viewer/code-viewer.component';
import { JsonPipe } from '@angular/common';
import { GitHubService } from '../github/GitHubService';

const areaRoutes: Routes = [
    { path: 'mongodb', component: MongoDbComponent },
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
        CodeViewerComponent,
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
        MongoDbComponent,
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
        MongoDbService,
        RabbitMqService,
        AmqpService,
        RedisService,

        GitHubService
    ]
})
export class IntegrationModule {

}