import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiRequest } from 'src/app/common/rest-client/ApiRequest';
import { ServiceApi } from 'src/app/api/ServiceApi';
import { ApiResponse } from 'src/app/common/rest-client/ApiResponse';
import { CreateClaimSubmission, ClaimStatusUpdated } from './models';

@Injectable()
export class AmqpService {

    constructor(
        private api: ServiceApi) {
    }
    
    public sendCommand(command: CreateClaimSubmission): Observable<ApiResponse<void>> {
        let link = this.api.apiEntry.getLink('amqp-send-command');
        let request = ApiRequest.fromLink(link, config => config.withContent(command))

        return this.api.client.send(request);
    }

    public publishDomainEvent(event: ClaimStatusUpdated): Observable<ApiResponse<void>> {
        let link = this.api.apiEntry.getLink('amqp-publish-event');
        let request = ApiRequest.fromLink(link, config => config.withContent(event))

        return this.api.client.send(request);
    }
}