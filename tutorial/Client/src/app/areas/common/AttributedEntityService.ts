import { ServiceApi } from 'src/app/api/ServiceApi';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiResponse } from 'src/app/common/rest-client/ApiResponse';
import { ApiRequest } from 'src/app/common/rest-client/ApiRequest';

@Injectable()
export class AttributedEntityService {

    constructor(
        private api: ServiceApi) {
    }

    public ReadAttributedEntity(): Observable<ApiResponse<void>> {
        let link = this.api.apiEntry.getLink('read-attributed-entity');
        return this.api.client.send(ApiRequest.fromLink(link));
    } 

    public UpdateAttributedEntity(values: any): Observable<ApiResponse<void>> {
        let link = this.api.apiEntry.getLink('update-attributed-entity');
        return this.api.client.send(ApiRequest.fromLink(link, config => config.withContent(values)));
    } 

    public ReadAttributesDynamically(): Observable<ApiResponse<void>> {
        let link = this.api.apiEntry.getLink('dynamically-read-attributes');
        return this.api.client.send(ApiRequest.fromLink(link));
    } 
}