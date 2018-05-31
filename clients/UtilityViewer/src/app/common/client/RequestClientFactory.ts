import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { RequestClient } from './RequestClient';
import { ApiRequest } from './Request';
import { RequestSettings } from './Settings';
import { IHalEntryPointResource } from './Resource';
import { Observable } from 'rxjs';


@Injectable()
export class RequestClientFactory {

    private _clients: {[name: string]: RequestClient} = {};
    private _entryResources: {[name: string]: IHalEntryPointResource } = {};

    public constructor(
        private httpClient: HttpClient) {

        
    }

    public createClient(name: string, baseAddress: string): RequestClient {
        var defaultSettings = RequestSettings.create((settings) => {
            settings.useHalDefaults();
        });

        this._clients[name] = new RequestClient(this.httpClient, baseAddress, defaultSettings);
        return this._clients[name];
    }

    public getClient(name: string): RequestClient {
        let client = this._clients[name];
        if (client == null) {

        }
        return client;
    }

    public getEntryPointResource(name: string, entryPath: string): Observable<IHalEntryPointResource> {
        var entryPointResource = this._entryResources[name];
        if (!entryPointResource) {
            return this.loadEntryPointResource(name, entryPath).map((entryPoint) => {
                this._entryResources[name] = entryPoint;
                return entryPoint;
            });
        }
        return Observable.of(entryPointResource);
    }

    private loadEntryPointResource(name: string, entryPath: string): Observable<IHalEntryPointResource> {
        let client = this.getClient(name);

        let request = ApiRequest.get(entryPath);
        return client.send<IHalEntryPointResource>(request)
            .map( response => response.content);       
    }
}