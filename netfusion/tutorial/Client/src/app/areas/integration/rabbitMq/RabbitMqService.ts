import { Injectable } from '@angular/core';
import { ServiceApi } from 'src/app/api/ServiceApi';
import { ApiRequest } from 'src/app/common/rest-client/ApiRequest';
import { Observable } from 'rxjs';
import { ApiResponse } from 'src/app/common/rest-client/ApiResponse';
import { map } from 'rxjs/operators';
import { 
    PropertySold,
    AutoSaleCompleted, 
    TemperatureReading, 
    SendEmail, 
    CalculateAutoTax, 
    CalculatePropertyTax, 
    TaxCalc } from './models';

@Injectable()
export class RabbitMqService {

    constructor(
        private api: ServiceApi) {
    }

    public publishDirectDomainEvent(event: PropertySold): Observable<ApiResponse<void>> {
        let link = this.api.apiEntry.getLink('rabbit-direct');
        let request = ApiRequest.fromLink(link, config => config.withContent(event))

        return this.api.client.send(request);
    }

    public publishTopicDomainEvent(event: AutoSaleCompleted): Observable<ApiResponse<void>> {
        let link = this.api.apiEntry.getLink('rabbit-topic');
        let request = ApiRequest.fromLink(link, config => config.withContent(event))

        return this.api.client.send(request);
    }

    public publishFanoutDomainEvent(event: TemperatureReading): Observable<ApiResponse<void>> {
        let link = this.api.apiEntry.getLink('rabbit-fanout');
        let request = ApiRequest.fromLink(link, config => config.withContent(event))

        return this.api.client.send(request);
    }

    public sendQueuedCommand(command: SendEmail): Observable<ApiResponse<void>> {
        let link = this.api.apiEntry.getLink('rabbit-queue');
        let request = ApiRequest.fromLink(link, config => config.withContent(command))

        return this.api.client.send(request);
    }

    public sendRpcAutoTaxCommand(command: CalculateAutoTax): Observable<TaxCalc> {
        let link = this.api.apiEntry.getLink('rabbit-rpc-auto');
        let request = ApiRequest.fromLink(link, config => config.withContent(command))

        return this.api.client.send<TaxCalc>(request).pipe(
            map((result) => result.content)
        );
    }

    public sendRpcPropertyTaxCommand(command: CalculatePropertyTax): Observable<TaxCalc> {
        let link = this.api.apiEntry.getLink('rabbit-rpc-property');
        let request = ApiRequest.fromLink(link, config => config.withContent(command))

        return this.api.client.send<TaxCalc>(request).pipe(
            map((result) => result.content)
        );
    }
}