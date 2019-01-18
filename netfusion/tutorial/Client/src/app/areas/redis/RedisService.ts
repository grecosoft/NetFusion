import { Injectable } from '@angular/core';
import { ServiceApi } from 'src/app/api/ServiceApi';
import { OrderSubmitted, SetValue } from './models';
import { Observable } from 'rxjs';
import { ApiResponse } from 'src/app/common/rest-client/ApiResponse';
import { ApiRequest } from 'src/app/common/rest-client/ApiRequest';
import { map } from 'rxjs/operators';

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

    public setValue(value: string): Observable<ApiResponse<void>> {
        let link = this.api.apiEntry.getLink('set-value');
        let request = ApiRequest.fromLink(link, config => config.withContent({value: value}))

        return this.api.client.send(request);
    }

    public popValue(): Observable<SetValue> {
        let link = this.api.apiEntry.getLink('pop-value');
        let request = ApiRequest.fromLink(link, config => config.withContent(event))

        return this.api.client.send<SetValue>(request).pipe(
            map(result => result.content)
        )
    }
}