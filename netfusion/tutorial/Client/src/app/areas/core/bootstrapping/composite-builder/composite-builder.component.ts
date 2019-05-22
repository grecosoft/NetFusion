import { Component } from '@angular/core';

@Component({
    templateUrl: './composite-builder.component.html',
    styleUrls: ['../../../area.scss']
})
export class CompositeBuilderComponent {
    public nugetPackageName = "NetFusion.AMQP";
    public documentationUrl = "https://github.com/grecosoft/NetFusion/wiki/integration.amqp.publisher#amqp-publisher-overview";
}