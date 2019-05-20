import { Component } from '@angular/core';

@Component({
    templateUrl: './queries.component.html',
    styleUrls: ['../../area.scss']
})
export class QueriesComponent {
    public nugetPackageName = "NetFusion.AMQP";
    public documentationUrl = "https://github.com/grecosoft/NetFusion/wiki/integration.amqp.publisher#amqp-publisher-overview";
}