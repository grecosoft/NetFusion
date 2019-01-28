import { NgModule } from '@angular/core';
import { SettingsComponent } from './settings/settings.component/settings.component';
import { AreaCommonModule } from './area-common.module';
import { Routes, RouterModule } from '@angular/router';
import { SettingsService } from './settings/SettingsService';
import { MongoDbComponent } from './mongoDb/mongoDb.component/mongoDb.component';
import { MongoDbService } from './mongoDb/MongoDbService';
import { JsonPipe } from '@angular/common';
import { RabbitMqDirectComponent } from './rabbitMq/rabbitMq-direct.component/rabbitMq-direct.component';
import { RabbitMqTopicComponent } from './rabbitMq/rabbitMq-topic.component/rabbitMq-topic.component';
import { RabbitMqFanoutComponent } from './rabbitMq/rabbitMq-fanout.component/rabbitMq-fanout.component';
import { RabbitMqQueueComponent } from './rabbitMq/rabbitMq-queue.component/rabbitMq-queue.component';
import { RabbitMqRpcComponent } from './rabbitMq/rabbitMq-rpc.component/rabbitMq-rpc.component';
import { RabbitMqService } from './rabbitMq/RabbitMqService';
import { GitHubService } from './github/GitHubService';
import { CodeViewerComponent } from './github/code-viewer/code-viewer.component';
import { RedisDataComponent } from './redis/redis-data.component/redis-data.component';
import { RedisService } from './redis/RedisService';
import { CompositeLogComponent } from './overview/composite-log/composite-log.component';
import { OverviewService } from './overview/OverviewService';
import { RedisChannelsComponent } from './redis/redis-channels.component/redis-channels.component';

const areaRoutes: Routes = [
    { path: 'settings', component: SettingsComponent },
    { path: 'mongodb', component: MongoDbComponent },
    { path: 'rabbitmq/direct', component: RabbitMqDirectComponent },
    { path: 'rabbitmq/topic', component: RabbitMqTopicComponent },
    { path: 'rabbitmq/fanout', component: RabbitMqFanoutComponent },
    { path: 'rabbitmq/queue', component: RabbitMqQueueComponent },
    { path: 'rabbitmq/rpc', component: RabbitMqRpcComponent },
    { path: 'redis/data', component: RedisDataComponent },
    { path: 'redis/channels', component: RedisChannelsComponent },
    { path: 'overview/composite-log', component: CompositeLogComponent }
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

        SettingsComponent,
        MongoDbComponent,

        RabbitMqDirectComponent,
        RabbitMqTopicComponent,
        RabbitMqFanoutComponent,
        RabbitMqQueueComponent,
        RabbitMqRpcComponent,

        RedisDataComponent,
        RedisChannelsComponent,

        CompositeLogComponent
    ],
    entryComponents: [
        SettingsComponent,
        MongoDbComponent,

        RabbitMqDirectComponent,
        RabbitMqTopicComponent,
        RabbitMqFanoutComponent,
        RabbitMqQueueComponent,
        RabbitMqRpcComponent,

        RedisDataComponent,
        RedisChannelsComponent,
        
        CompositeLogComponent
    ],
    providers: [
        JsonPipe,
        SettingsService,
        MongoDbService,
        RabbitMqService,
        GitHubService,
        RedisService,
        OverviewService
    ]
})
export class ExamplesModule {

}