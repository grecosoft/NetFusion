import { Component } from '@angular/core';
import { RabbitMqService } from '../RabbitMqService';
import { PropertySold } from '../models';
import { JsonPipe } from '@angular/common';
import { ApiResponse } from 'src/app/common/rest-client/ApiResponse';

@Component({
    templateUrl: './rabbitMq-direct.component.html',
    styleUrls: ['../../../area.scss']
})
export class RabbitMqDirectComponent {

    public nugetPackageName = "NetFusion.RabbitMQ";
    public documentationUrl = "https://github.com/grecosoft/NetFusion/wiki/integration.rabbitmq.direct#rabbitmq---direct-exchanges";
    public exchangeUrl = "http://localhost:15682/#/exchanges/netfusion/RealEstate";

    public newDomainEvent: PropertySold;
    public newDomainEventBody: string;
    public newDomainEventResult: ApiResponse<void>;

    public northEastQueueUrl = "http://localhost:15682/#/queues/netfusion/NorthEast-%3E(AD13D4BB-8777-4F73-8C6E-23BD03ABC433)";
    public southEastQueueUrl = "http://localhost:15682/#/queues/netfusion/SouthEast-%3E(AD13D4BB-8777-4F73-8C6E-23BD03ABC433)";

    constructor(
        private json: JsonPipe,
        private rabbitMqService: RabbitMqService) {

        this.newDomainEvent = {
            address: "444 West Main Street",
            city: "Cheshire",
            state: "CT",
            zip: "06410",
            askingPrice: 400000,
            soldPrice: 385000
        };

        this.newDomainEventBody = this.json.transform(this.newDomainEvent)
    }

    public onPublishDomainEvent() {
        let domainEvent = <PropertySold>JSON.parse(this.newDomainEventBody);

        this.rabbitMqService.publishDirectDomainEvent(domainEvent)
            .subscribe((response) => {
                this.newDomainEventResult = response;
            });
    }
}