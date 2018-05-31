import {HttpClient, HttpHeaders, HttpParams, HttpResponse} from "@angular/common/http";
import { Observable } from "rxjs";
import 'rxjs/add/operator/map';

import { ApiRequest, ApiResponse } from './Request'
import { RequestSettings } from './Settings'

import * as _ from 'lodash';

export class RequestClient  {
    
    constructor(
        private http:HttpClient, 
        private baseAddress: string,
        private defaultSettings: RequestSettings) {
    }

    public send<TContent>(request: ApiRequest): Observable<ApiResponse<TContent>> {

        let options = this.buildOptions(request);

        let url = '/';
        if (this.baseAddress.endsWith('/') || request.requestUri.startsWith('/')) {
            url = '';
        }

        url = this.baseAddress + url + request.requestUri;

        return this.http.request<TContent>(request.method, url, options)
            .map((response: any) => {
               let apiResponse = new ApiResponse<TContent>(response);
               return apiResponse;
            });
    }

    private buildOptions(request: ApiRequest): { headers: HttpHeaders, params: {[name: string]:string}, body: any} {

        let settings = this.defaultSettings.merge(request.settings);

        if (request.embeddedNames) {
            settings.queryString.addParam('embedded', request.embeddedNames);
        }

        let options = {
            headers: new HttpHeaders(settings.headers.getHeaders()),
            params: settings.queryString.getParams(),
            body: request.content,
            observe: 'response'
        };

        return options;
    }
}