import { Component } from '@angular/core';

@Component({
    templateUrl: './enrichers.component.html',
    styleUrls: ['../../area.scss']
})
export class EnrichersComponent {
    public nugetPackageName = "NetFusion.AMQP";
    public documentationUrl = "https://github.com/grecosoft/NetFusion/wiki/integration.amqp.publisher#amqp-publisher-overview";
}