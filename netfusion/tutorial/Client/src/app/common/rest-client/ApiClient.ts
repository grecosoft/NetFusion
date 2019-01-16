import { HttpClient, HttpHeaders, HttpErrorResponse, HttpResponse } from "@angular/common/http";
import { Observable, throwError, EMPTY } from "rxjs";
import {map, catchError} from 'rxjs/operators';

import { ApiRequest } from './ApiRequest'
import { ApiResponse } from './ApiResponse'
import { RequestSettings } from './RequestSettings'
import { AuthResult } from "./AuthResult";

export class ApiClient  {
    
    private authHandler: (authUrl: string) => Observable<AuthResult>;
    private forbiddenHandler: () => Observable<void>;

    constructor(
        public http:HttpClient, 
        public baseAddress: string,
        private defaultSettings: RequestSettings) {
    }

    // Sets a method that can be invoked when a HTTP 401 is detected.
    // The method takes the url to call to authenticate and returns
    // an observable that can be subscribed to monitor login results.
    public setAuthHandler(handler: (authUrl: string) => Observable<AuthResult>) {
        this.authHandler = handler;
    }

    public setForbiddenHandler(handler: () => Observable<void>) {
        this.forbiddenHandler = handler;
    }

    public send<TContent>(request: ApiRequest): Observable<ApiResponse<TContent>> {

        let options = this.buildOptions(request);

        let url = '/';
        if (this.baseAddress.endsWith('/') || request.requestUri.startsWith('/')) {
            url = '';
        }

        url = this.baseAddress + url + request.requestUri;

        return this.http.request<HttpResponse<TContent>>(request.method, url, options)
            .pipe(              
                map((response: HttpResponse<TContent>) => {
                    let apiResponse = new ApiResponse<TContent>(response);
                    return apiResponse;
                }),
                
                catchError((err, caught) => {

                    if (err.status === 401) {
                        return this.handleUnAuthorizedError<TContent>(err, url, request);
                    } else if(err.status === 403) {
                        return this.handleForbiddenError<TContent>();
                    }

                    return throwError(err);
                })
            );
    }

    private buildOptions(request: ApiRequest): { 
        // The structure of the returned type:
        headers: HttpHeaders,
        params: {[name: string]:string}, 
        body: any} {

        let settings = request.settings ? this.defaultSettings.merge(request.settings) 
            : this.defaultSettings;

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

    // Returns an observable that can be observed to determine how the
    // error should be handled.
    private handleUnAuthorizedError<TContent>(error: HttpErrorResponse, url: string, request: ApiRequest)
        : Observable<ApiResponse<TContent>> {

        // Only handle 401 status code and only handle if a authentication handler
        // has been provided when the client was created.
        if (this.authHandler == null) {
            return throwError("Error completing API request.")
        }

        // Get the URL that should be invoked to authenticate and subscribe to the
        // observable that will result in the auth token or error.
        let authUrl = this.getBearerRealm(error.headers);
        if (authUrl == null) {
            return throwError("Error authenticating request.")
        }

        return new Observable<ApiResponse<TContent>>((o) => {

            // Invoke the provided authentication handler to allow the caller to 
            // prompt the user for their credentials and authenticate using the
            // URL returned from the service that issued the HTTP 401.
            this.authHandler(authUrl).subscribe((authResult) => {
    
                // If the user was able to authenticate, retry the request.
                let options = this.buildOptions(request);

                this.http.request<HttpResponse<TContent>>(request.method, url, options)
                    .pipe(              
                        map((response: HttpResponse<TContent>) => {
                            let apiResponse = new ApiResponse<TContent>(response);
                            return apiResponse;
                        })
                    ).subscribe(
                        (resp: ApiResponse<TContent>) => o.next(resp)
                    ); 
                }
            );
        });
    }

    private handleForbiddenError<TContent>(): Observable<ApiResponse<TContent>> {

        if (this.forbiddenHandler == null) {
            return throwError("Error completing API request.")
        }

        return new Observable<ApiResponse<TContent>>(o => {
            return o.next();
        });
    }

    // Returns the URL from the returned www-authenticate that should
    // be called to authenticate the caller.
    private getBearerRealm(headers: HttpHeaders): string {
        if (! headers.has('www-authenticate')) {
            return;
        }

        let authHeader: string = headers.get('www-authenticate');
        let authHeaderValues = authHeader.replace(' ', '').split(',');

        if (authHeaderValues.length === 0) {
            return;
        }
        
        authHeaderValues = authHeaderValues[0].split('=');
        if (authHeaderValues.length !== 2 || authHeaderValues[0] !== 'realm') {
            return null;
        }

        return authHeaderValues[1];
    }
}