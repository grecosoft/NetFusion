import { Component } from '@angular/core';
import { RedisService } from '../RedisService';
import { OrderSubmitted } from '../models';
import { ApiResponse } from 'src/app/common/rest-client/ApiResponse';
import { JsonPipe } from '@angular/common';


@Component({
    templateUrl: './redis-channels.component.html',
    styleUrls: ['../../area.scss'] 
})
export class RedisChannelsComponent {

    //public injectedSettings: CalculationSettings;
    public nugetPackageName = "NetFusion.Redis";
    public documentationUrl = "https://github.com/grecosoft/NetFusion/wiki/integration.redis.overview#redis-pubsub";
    public containerScope = "Singleton";

    public newDomainEvent: OrderSubmitted;
    public newDomainEventBody: string;
    public newDomainEventResult: ApiResponse<void>;

    constructor(
        private json: JsonPipe,
        private redisService: RedisService) {

        this.newDomainEvent = {
           partNumber: "A34534534",
           quantity: 20,
           state: "CT"
        };

        this.newDomainEventBody = this.json.transform(this.newDomainEvent)
    }

    public onPublishDomainEvent() {
        let domainEvent = <OrderSubmitted>JSON.parse(this.newDomainEventBody);

        this.redisService.publishDomainEvent(domainEvent)
            .subscribe((response) => {
                this.newDomainEventResult = response;
            });
    }

}