import { Component } from '@angular/core';
import { RabbitMqService } from '../RabbitMqService';
import { PropertySold, TemperatureReading } from '../models';
import { JsonPipe } from '@angular/common';
import { ApiResponse } from 'src/app/common/rest-client/ApiResponse';

@Component({
    templateUrl: './rabbitMq-fanout.component.html',
    styleUrls: ['../../area.scss']
})
export class RabbitMqFanoutComponent {

    public nugetPackageName = "NetFusion.RabbitMQ";
    public documentationUrl = "https://github.com/grecosoft/NetFusion/wiki/integration.rabbitmq.fanout#rabbitmq---fan-out-exchanges";
    public exchangeUrl="http://localhost:15682/#/exchanges/netfusion/TemperatureReading";

    public newDomainEvent: TemperatureReading;
    public newDomainEventBody: string;
    public newDomainEventResult: ApiResponse<void>;

    constructor(
        private json: JsonPipe,
        private rabbitMqService: RabbitMqService) {

        this.newDomainEvent = {
            zip: "27517",
            reading: 72.254,
            coordinates: {
                north: 35.9132,
                west: 79.0558
            }
        };

        this.newDomainEventBody = this.json.transform(this.newDomainEvent)
    }

    public onPublishDomainEvent() {
        let domainEvent = <TemperatureReading>JSON.parse(this.newDomainEventBody);

        this.rabbitMqService.publishFanoutDomainEvent(domainEvent)
            .subscribe((response) => {
                this.newDomainEventResult = response;
            });
    }
}