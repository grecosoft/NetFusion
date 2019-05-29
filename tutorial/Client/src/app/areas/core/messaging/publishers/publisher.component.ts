import { Component } from '@angular/core';

@Component({
    templateUrl: './publisher.component.html',
    styleUrls: ['../../../area.scss']
})
export class PublishersComponent {
    public nugetPackageName = "NetFusion.AMQP";
    public documentationUrl = "https://github.com/grecosoft/NetFusion/wiki/integration.amqp.publisher#amqp-publisher-overview";
}