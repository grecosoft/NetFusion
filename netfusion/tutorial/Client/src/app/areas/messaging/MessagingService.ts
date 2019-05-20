import { Injectable } from '@angular/core';
import { ServiceApi } from 'src/app/api/ServiceApi';
import { ApiRequest } from 'src/app/common/rest-client/ApiRequest';
import { Observable } from 'rxjs';
import { ApiResponse } from 'src/app/common/rest-client/ApiResponse';
import { RangeModel, AccountModel } from './models';

@Injectable()
export class MessagingService {

    constructor(
        private api: ServiceApi) {
    }

    public sendCommand(model: RangeModel): Observable<ApiResponse<Range>> {
        let link = this.api.apiEntry.getLink('messaging-command');
        let request = ApiRequest.fromLink(link, config => config.withContent(model))

        return this.api.client.send(request);
    }

    public publishDomainEvent(model: AccountModel): Observable<ApiResponse<string[]>> {
        let link = this.api.apiEntry.getLink('messaging-domain-event');
        let request = ApiRequest.fromLink(link, config => config.withContent(model))

        return this.api.client.send(request);
    }
}