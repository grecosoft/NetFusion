import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable, of } from "rxjs";
import { map, switchMap } from "rxjs/operators";

import { ApiClient } from "src/app/common/rest-client/ApiClient";
import { ApiRequest } from "src/app/common/rest-client/ApiRequest";
import { RequestSettings } from "src/app/common/rest-client/RequestSettings";
import { ApiResponse } from "src/app/common/rest-client/ApiResponse";
import { AuthResult } from "src/app/common/rest-client/AuthResult";
import { HalResource, IHalEntryPointResource } from "src/app/common/rest-client/Resources";

import { LocalStorageService } from "../common/services/LocalStorageService";
import { DialogService } from '../common/dialogs/DialogService';
import { LoginDialogComponent } from '../common/dialogs/login-dialog/login-dialog.component';

// Contains logic use to load an application's API entry resource and
// contains code to authenticate the user when the delegated to ApiClient
// receives an HTTP 401.
@Injectable()
export class ServiceApi {

    // Current service client and its associated entry resource.
    public client: ApiClient;
    public apiEntry: HalResource;

    public isAuthenticated: boolean;
    private defaultSettings: RequestSettings;

    // Notifications:
    public constructor(
        private localStorage: LocalStorageService,
        private httpClient: HttpClient,
        private dialogService: DialogService) {
    }

    // Loads a resource containing links used to initiate communication with the application's Api.  
    // After links are invoked provided by this resource, subsequent returned resources will have their
    // own associated links used to modify the resource or load related resources.
    public loadServiceEntry(baseUrl: string, entryPath: string): Observable<IHalEntryPointResource> {

        // Get the cached JWT token if present.
        let jwtToken = this.getJwtToken(baseUrl);

        // These are the default settings that will be used for each 
        // request if not overridden when making a specific request.                
        this.defaultSettings = RequestSettings.create((config: RequestSettings) => {

            config.useHalDefaults();
            if (jwtToken) {
                config.headers.addHeader('Authorization', 'Bearer ' + jwtToken);
            }
        });

        // Create the REST/HAL client which is just a wrapper around the Angular HttpClient.
        this.client = new ApiClient(this.httpClient,
            baseUrl,  
            this.defaultSettings);

        // Sets the method that will be called with a HTTP 401 status code is returned from the server. 
        this.client.setAuthHandler((authUrl) => this.authenticateUser(authUrl, this.httpClient));
        // this.management.setForbiddenHandler(() => this.alertForbiddenUser());

        return this.loadApiEntry(entryPath).pipe(
            switchMap((entryResource: IHalEntryPointResource) => {

                // Ping the server to validate cached jwt token if present.
                return this.pingServer().pipe(
                    map((isAuthenticated: boolean) => {
                        this.isAuthenticated = isAuthenticated;
                        return entryResource;
                    })
                )
            })
        );
    }

    private getJwtToken(baseUrl: string): string {

        let jwtStore = this.localStorage.load<JwtStorage>(JwtStorage.storeKey) || new JwtStorage();
        return jwtStore.tokens[baseUrl]
    }

    private loadApiEntry(entryPath: string): Observable<IHalEntryPointResource> {

        return this.client.send<IHalEntryPointResource>(
            ApiRequest.get(entryPath)).pipe(

                map(result => {
                    this.apiEntry = HalResource.for(result.content);
                    return result.content;
                })
            );
    }

    private pingServer(): Observable<boolean>
    {
        if (this.defaultSettings.headers.hasHeader('Authorization')) {

            return this.ping().pipe(
    
                map(result => {
                    return result.response.status !== 401;;
                })
            );
        }

        return of(false);
    }

    // If the entry resources has a defined 'ping' link, it is invoked.  On the server,
    // this should refer to a API method requiring authentication.  This can be used 
    // when the application bootstraps to determine if the lastly stored JWT token
    // is still valid.  If this method returns a HTTP 401, the error handler will be
    // invoked for this status code requiring the user to login.
    public ping() : Observable<ApiResponse<any>> {
        if (this.apiEntry.hasLink('ping')) {
            let pingRequest = ApiRequest.fromLink(this.apiEntry.getLink('ping'));
            return this.client.send<any>(pingRequest);
        }

        return of();
    }

    public clearJwtTokens() {
        this.localStorage.remove(JwtStorage.storeKey);
        this.isAuthenticated = false;
        this.defaultSettings.headers.removeHeader('Authorization');
    }

    private saveJtwToken(baseUrl: string, token: string) {

        let jwtStore = this.localStorage.load<JwtStorage>(JwtStorage.storeKey) || new JwtStorage();

        jwtStore.tokens[baseUrl] = token;
        this.localStorage.save(JwtStorage.storeKey, jwtStore);
    }

    // Called by the ApiClient with a HTTP 401 status is detected.  This method 
    // is passed the URL to which the user should try to authenticate.  
    private authenticateUser(authUrl: string, httpClient: HttpClient): Observable<AuthResult> {

        let login = new Observable<AuthResult>((o) => {

            let result = new AuthResult();

            const dialogRef = this.dialogService.openDialog(LoginDialogComponent, { }, { width: '360px'} );

            // Subscribe to the dialog component to observer user login attempts or when
            // they they cancel the login process.
            dialogRef.componentInstance.loginAttempted.subscribe({
                next: (loginData) => {

                    // Construct the basic authentication header:
                    let authHeader = {
                        'Authorization': 'Basic ' + btoa(`${loginData.username}:${loginData.password}`)};

                    // Submit the credentials:
                    httpClient.get(authUrl, { headers: authHeader, observe: 'response' }).subscribe((resp) => {

                        if (resp.headers.has('x-custom-token')) {
                            result.token = resp.headers.get('x-custom-token');

                            // Add the JWT token to the default request setting to be used for future requests:
                            this.defaultSettings.headers.addHeader('Authorization', 'Bearer ' + result.token)

                            // Store token so that it can be used if the application is refreshed
                            // or closed and reopened and also add it to the default settings:
                            this.saveJtwToken(this.client.baseAddress, result.token)
                            this.isAuthenticated = true;

                            // Close the dialog and notify observer of the result:
                            dialogRef.close();
                            o.next(result);
                        }
                    }, (error) => {
                        if (error.status == 401) {
                            
                            // Notify the user that their provided credentials were
                            // not valid.
                            dialogRef.componentInstance.setMessage("Invalid Credentials");
                        } else {
                            o.error(error);
                        }
                    });
                },
                complete: () => dialogRef.close() // User canceled dialog.
            }); 
        });

        // Return observable to caller can observe the result of the login.
        return login;
    }
}

export class JwtStorage {

    public static get storeKey(): string {
        return "app:jwt-tokens"
    }

    public tokens: Map<string, string> = new Map<string, string>();
}
