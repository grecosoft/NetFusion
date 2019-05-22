import { Component } from '@angular/core';

@Component({
    templateUrl: './rules.component.html',
    styleUrls: ['../../../area.scss']
})
export class RulesComponent {
    public nugetPackageName = "NetFusion.AMQP";
    public documentationUrl = "https://github.com/grecosoft/NetFusion/wiki/integration.amqp.publisher#amqp-publisher-overview";
}