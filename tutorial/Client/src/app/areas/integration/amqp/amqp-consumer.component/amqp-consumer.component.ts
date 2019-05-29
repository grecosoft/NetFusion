import { Component } from '@angular/core';

@Component({
    templateUrl: './amqp-consumer.component.html',
    styleUrls: ['../../../area.scss']
})
export class AmqpConsumerComponent {
    public nugetPackageName = "NetFusion.AMQP";
    public documentationUrl = "https://github.com/grecosoft/NetFusion/wiki/integration.amqp.subscriber#amqp-subscriber-overview";
}