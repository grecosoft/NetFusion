import { Injectable } from '@angular/core';
import { ServiceApi } from 'src/app/api/ServiceApi';
import { ApiResponse } from 'src/app/common/rest-client/ApiResponse';
import { Observable } from 'rxjs';
import { ApiRequest } from 'src/app/common/rest-client/ApiRequest';

@Injectable()
export class RoslynService {

    constructor(
        private api: ServiceApi) {
    }

    public applyExpressionsToEntity(entity: any): Observable<ApiResponse<void>> {
        let link = this.api.apiEntry.getLink('evaluate-sensor');
        return this.api.client.send(ApiRequest.fromLink(link, config => config.withContent(entity)));
    } 
}