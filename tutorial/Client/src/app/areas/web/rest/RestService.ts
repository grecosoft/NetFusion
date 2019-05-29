import { Injectable } from '@angular/core';
import { ServiceApi } from 'src/app/api/ServiceApi';
import { ApiResponse } from 'src/app/common/rest-client/ApiResponse';
import { Observable } from 'rxjs';
import { Link } from 'src/app/common/rest-client/Resources';
import { ApiRequest } from 'src/app/common/rest-client/ApiRequest';

@Injectable()
export class RestService {

    constructor(
        private api: ServiceApi) {
    }

    public loadSchool(id: number): Observable<ApiResponse<any>> {
        let link: Link = { href: `api/schools/${id}`, methods: ["GET"], templated: false};
        let request = ApiRequest.fromLink(link)

        return this.api.client.send(request);
    } 

    public loadStudents(id: number): Observable<ApiResponse<any>> {
        let link: Link = { href: `api/schools/${id}/students`, methods: ["GET"], templated: false};
        let request = ApiRequest.fromLink(link)

        return this.api.client.send(request);
    } 

}