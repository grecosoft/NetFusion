import { Component } from '@angular/core';
import { AccountModel } from '../models';
import { ApiResponse } from 'src/app/common/rest-client/ApiResponse';
import { JsonPipe } from '@angular/common';
import { MessagingService } from '../MessagingService';

@Component({
    templateUrl: './domain-events.component.html',
    styleUrls: ['../../../area.scss']
})
export class DomainEventsComponent {
    public nugetPackageName = "NetFusion.AMQP";
    public documentationUrl = "https://github.com/grecosoft/NetFusion/wiki/integration.amqp.publisher#amqp-publisher-overview";

    public newDomainEvent: AccountModel;
    public newDomainEventBody: string;
    public newDomainEventResult: ApiResponse<string[]>;

    constructor(
        private json: JsonPipe,
        private messagingService: MessagingService) {

        this.newDomainEvent = {
            firstName: "Jon",
            lastName: "Smith",
            accountNumber: "892347289347234"
        };

        this.newDomainEventBody = this.json.transform(this.newDomainEvent)
    }

    public onPublishDomainEvent() {
        let domainEvent = <AccountModel>JSON.parse(this.newDomainEventBody);

        this.messagingService.publishDomainEvent(domainEvent)
            .subscribe((response) => {
                this.newDomainEventResult = response;
            });
    }
}