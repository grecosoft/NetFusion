import { Component } from '@angular/core';
import { RabbitMqService } from '../RabbitMqService';
import { CalculateAutoTax, TaxCalc, CalculatePropertyTax } from '../models';
import { JsonPipe } from '@angular/common';

@Component({
    templateUrl: './rabbitMq-rpc.component.html',
    styleUrls: ['../../../area.scss']
})
export class RabbitMqRpcComponent {

    public nugetPackageName = "NetFusion.RabbitMQ";
    public documentationUrl = "https://github.com/grecosoft/NetFusion/wiki/integration.rabbitmq.rpc#rabbitmq---rpc";
    public queueUrl = "http://localhost:15682/#/queues/netfusion/TaxCalculations";

    public newAutoCommand: CalculateAutoTax;
    public newAutoCommandBody: string;

    public newPropertyCommand: CalculatePropertyTax;
    public newPropertyCommandBody: string;

    public newCommandResult: TaxCalc;

    constructor(
        private json: JsonPipe,
        private rabbitMqService: RabbitMqService) {

        this.newAutoCommand = {
            vin: "3434V3453B345N",
            zipCode: "06415"
        };

        this.newPropertyCommand = {
            address: "535 Wood Hill Road",
            city: "Cheshire",
            state: "NC",
            zip: "06410"
        }

        this.newAutoCommandBody = this.json.transform(this.newAutoCommand);
        this.newPropertyCommandBody = this.json.transform(this.newPropertyCommand);
    }

    public onSendAutoCommand() {
        let command = <CalculateAutoTax>JSON.parse(this.newAutoCommandBody);

        this.rabbitMqService.sendRpcAutoTaxCommand(command)
            .subscribe((response) => {
                this.newCommandResult = response;
            });
    }

    public onSendPropertyCommand() {
        let command = <CalculatePropertyTax>JSON.parse(this.newPropertyCommandBody);

        this.rabbitMqService.sendRpcPropertyTaxCommand(command)
            .subscribe((response) => {
                this.newCommandResult = response;
            });
    }
}