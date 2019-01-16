import { Injectable } from '@angular/core';
import { ServiceApi } from 'src/app/api/ServiceApi';
import { OrderSubmitted } from './models';
import { Observable } from 'rxjs';
import { ApiResponse } from 'src/app/common/rest-client/ApiResponse';
import { ApiRequest } from 'src/app/common/rest-client/ApiRequest';

@Injectable()
export class RedisService {

    constructor(
        private api: ServiceApi) {
    }

    public publishDomainEvent(event: OrderSubmitted): Observable<ApiResponse<void>> {
        let link = this.api.apiEntry.getLink('redis-channel');
        let request = ApiRequest.fromLink(link, config => config.withContent(event))

        return this.api.client.send(request);
    }
}