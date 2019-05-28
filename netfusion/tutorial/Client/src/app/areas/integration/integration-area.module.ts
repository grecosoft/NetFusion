import { NgModule } from '@angular/core';
import { AmqpConsumerComponent } from './amqp/amqp-consumer.component/amqp-consumer.component';
import { AmqpPublisherComponent } from './amqp/amqp-publisher.component/amqp-publisher.component';
import { RedisChannelsComponent } from './redis/redis-channels.component/redis-channels.component';
import { RedisDataComponent } from './redis/redis-data.component/redis-data.component';
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
import { AreaModule } from '../area.module';
import { JsonPipe } from '@angular/common';
import { EntityFrameworkComponent } from './entityFramework/entityframework.component';
import { RoslynExpressionsComponent } from './roslyn/roslyn-expressions.component';
import { RoslynService } from './roslyn/RoslynService';

const areaRoutes: Routes = [
    { path: 'database/mongodb', component: MongoDbComponent },
    { path: 'database/entityframework', component: EntityFrameworkComponent },

    { path: 'rabbitmq/direct', component: RabbitMqDirectComponent },
    { path: 'rabbitmq/topic', component: RabbitMqTopicComponent },
    { path: 'rabbitmq/fanout', component: RabbitMqFanoutComponent },
    { path: 'rabbitmq/queue', component: RabbitMqQueueComponent },
    { path: 'rabbitmq/rpc', component: RabbitMqRpcComponent },

    { path: 'amqp/publisher', component: AmqpPublisherComponent },
    { path: 'amqp/consumer', component: AmqpConsumerComponent },

    { path: 'redis/data', component: RedisDataComponent },
    { path: 'redis/channels', component: RedisChannelsComponent },

    { path: 'roslyn/expressions', component: RoslynExpressionsComponent }
];

@NgModule({
    imports: [
        AreaModule,
        RouterModule.forChild(
            areaRoutes
          )
    ],
    declarations: [
        MongoDbComponent,
        EntityFrameworkComponent,

        RabbitMqDirectComponent,
        RabbitMqTopicComponent,
        RabbitMqFanoutComponent,
        RabbitMqQueueComponent,
        RabbitMqRpcComponent,

        RedisDataComponent,
        RedisChannelsComponent,

        AmqpPublisherComponent,
        AmqpConsumerComponent,

        RoslynExpressionsComponent
    ],
    entryComponents: [
        MongoDbComponent,
        EntityFrameworkComponent,

        RabbitMqDirectComponent,
        RabbitMqTopicComponent,
        RabbitMqFanoutComponent,
        RabbitMqQueueComponent,
        RabbitMqRpcComponent,

        RedisDataComponent,
        RedisChannelsComponent,

        AmqpPublisherComponent,
        AmqpConsumerComponent,

        RoslynExpressionsComponent
    ],
    providers: [
        JsonPipe,
        MongoDbService,
        RabbitMqService,
        AmqpService,
        RedisService,
        RoslynService
    ]
})
export class IntegrationAreaModule {

}