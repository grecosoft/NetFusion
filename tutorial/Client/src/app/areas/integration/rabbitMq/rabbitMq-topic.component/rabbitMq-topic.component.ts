import { Component } from '@angular/core';
import { RabbitMqService } from '../RabbitMqService';
import { PropertySold, AutoSaleCompleted } from '../models';
import { JsonPipe } from '@angular/common';
import { ApiResponse } from 'src/app/common/rest-client/ApiResponse';

@Component({
    templateUrl: './rabbitMq-topic.component.html',
    styleUrls: ['../../../area.scss']
})
export class RabbitMqTopicComponent {

    public nugetPackageName = "NetFusion.RabbitMQ";
    public documentationUrl = "https://github.com/grecosoft/NetFusion/wiki/integration.rabbitmq.topic#rabbitmq---topic-exchanges";
    public germanAutoSalesQueueUrl = "http://localhost:15682/#/queues/netfusion/GermanAutoSales-%3E(AD13D4BB-8777-4F73-8C6E-23BD03ABC433)";
    public americanAutoSalesQueueUrl = "http://localhost:15682/#/queues/netfusion/AmericanAutoSales-%3E(AD13D4BB-8777-4F73-8C6E-23BD03ABC433)";
    public sweetishAutoSalesUrl = "http://localhost:15682/#/queues/netfusion/SweetishAutoSales-%3E(AD13D4BB-8777-4F73-8C6E-23BD03ABC433)";

    public newDomainEvent: AutoSaleCompleted;
    public newDomainEventBody: string;
    public newDomainEventResult: ApiResponse<void>;

    constructor(
        private json: JsonPipe,
        private rabbitMqService: RabbitMqService) {

        this.newDomainEvent = {
            make: "VW",
            model: "GTI",
            year: 2017,
            color: "Black",
            isNew: true
        };

        this.newDomainEventBody = this.json.transform(this.newDomainEvent)
    }

    public onPublishDomainEvent() {
        let domainEvent = <AutoSaleCompleted>JSON.parse(this.newDomainEventBody);

        this.rabbitMqService.publishTopicDomainEvent(domainEvent)
            .subscribe((response) => {
                this.newDomainEventResult = response;
            });
    }
}