import { Component } from '@angular/core';
import { RangeModel } from '../models';
import { ApiResponse } from 'src/app/common/rest-client/ApiResponse';
import { JsonPipe } from '@angular/common';
import { MessagingService } from '../MessagingService';

@Component({
    templateUrl: './commands.component.html',
    styleUrls: ['../../area.scss']
})
export class CommandsComponent {
    public nugetPackageName = "NetFusion.AMQP";
    public documentationUrl = "https://github.com/grecosoft/NetFusion/wiki/integration.amqp.publisher#amqp-publisher-overview";

    public newCommand: RangeModel;
    public newCommandBody: string;
    public newCommandResult: ApiResponse<Range>;

    constructor(
        private json: JsonPipe,
        private messagingService: MessagingService) {

        this.newCommand = {
            template: 'The range is from {min} to {max}.',
            setOne: [5, 8, 9, 12, 5, 87],
            setTwo: [60, 4, 90, 22, 45]
        };

        this.newCommandBody = this.json.transform(this.newCommand)
    }

    public onSendCommand() {
        let command = <RangeModel>JSON.parse(this.newCommandBody);

        this.messagingService.sendCommand(command)
            .subscribe((response) => {
                this.newCommandResult = response;
            });
    }
}