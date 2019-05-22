import { Component } from '@angular/core';
import { ApiResponse } from 'src/app/common/rest-client/ApiResponse';
import { CreateClaimSubmission, ClaimStatusUpdated } from '../models';
import { JsonPipe } from '@angular/common';
import { AmqpService } from '../AmqpService';

@Component({
    templateUrl: './amqp-publisher.component.html',
    styleUrls: ['../../../area.scss']
})
export class AmqpPublisherComponent {
    public nugetPackageName = "NetFusion.AMQP";
    public documentationUrl = "https://github.com/grecosoft/NetFusion/wiki/integration.amqp.publisher#amqp-publisher-overview";

    public newCommand: CreateClaimSubmission;
    public newCommandBody: string;
    public commandResult: ApiResponse<void>;

    public newDomainEvent: ClaimStatusUpdated;
    public newDomainEventBody: string;
    public domainEventResult: ApiResponse<void>;

    constructor(
        private json: JsonPipe,
        private amqpService: AmqpService) {

        this.newCommand = {
            insuredId: "121212",
            insuredFirstName: "Alex",
            insuredLastName: "Smith",
            insuredDeductible: 2000,
            claimEstimate: 3000,
            claimDescription: "Back car into pole."
        }

        this.newDomainEvent = {
            insuredId: "121212",
            currentState: "Review",
            nextStatusUpdate: "3/3/2019",
            nextStatus: "Pending Payment"
        };

        this.newCommandBody = this.json.transform(this.newCommand);
        this.newDomainEventBody = this.json.transform(this.newDomainEvent)
    }

    public onSendCommand() {
        let command = <CreateClaimSubmission>JSON.parse(this.newCommandBody);

        this.amqpService.sendCommand(command)
            .subscribe((response) => {
                this.commandResult = response;
            });
    }

    public onPublishDomainEvent() {
        let domainEvent = <ClaimStatusUpdated>JSON.parse(this.newDomainEventBody);

        this.amqpService.publishDomainEvent(domainEvent)
            .subscribe((response) => {
                this.domainEventResult = response;
            });
    }
    
}