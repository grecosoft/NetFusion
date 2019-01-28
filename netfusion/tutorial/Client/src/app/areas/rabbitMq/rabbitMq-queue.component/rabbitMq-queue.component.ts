import { Component } from '@angular/core';
import { RabbitMqService } from '../RabbitMqService';
import { PropertySold, SendEmail } from '../models';
import { JsonPipe } from '@angular/common';
import { ApiResponse } from 'src/app/common/rest-client/ApiResponse';

@Component({
    templateUrl: './rabbitMq-queue.component.html',
    styleUrls: ['../../area.scss']
})
export class RabbitMqQueueComponent {

    public nugetPackageName = "NetFusion.RabbitMQ";
    public documentationUrl = "https://github.com/grecosoft/NetFusion/wiki/integration.rabbitmq.workqueue#rabbitmq---work-queues";
    public queueUrl = "http://localhost:15682/#/queues/netfusion/GeneratedAndSendEmail";

    public newCommand: SendEmail;
    public newCommandBody: string;
    public newCommandResult: ApiResponse<void>;

    constructor(
        private json: JsonPipe,
        private rabbitMqService: RabbitMqService) {

        this.newCommand = {
            subject: "Test Email",
            formAddress: "mark.twain@home.com",
            toAddresses: [
                "satya.nadella@microsoft.com",
                "steve.ballmer@glad_he_is_gone.com"
            ],
            message: "Thanks you for .net core."
        };

        this.newCommandBody = this.json.transform(this.newCommand)
    }

    public onSendCommand() {
        let command = <SendEmail>JSON.parse(this.newCommandBody);

        this.rabbitMqService.sendQueuedCommand(command)
            .subscribe((response) => {
                this.newCommandResult = response;
            });
    }
}