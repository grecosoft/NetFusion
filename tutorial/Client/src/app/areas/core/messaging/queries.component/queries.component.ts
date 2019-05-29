import { Component } from '@angular/core';
import { MessagingService } from '../MessagingService';

@Component({
    templateUrl: './queries.component.html',
    styleUrls: ['../../../area.scss']
})
export class QueriesComponent {
    public nugetPackageName = "NetFusion.AMQP";
    public documentationUrl = "https://github.com/grecosoft/NetFusion/wiki/integration.amqp.publisher#amqp-publisher-overview";

    public queryResults: any[];

    constructor(
        private messagingService: MessagingService) {
    }
    
    public onDispatchQuery() {
        this.messagingService.dispatchQuery()
            .subscribe((response) => {
                this.queryResults = response.content;
            });
    }
}