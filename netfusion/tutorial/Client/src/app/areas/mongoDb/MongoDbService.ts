import { Injectable } from '@angular/core';
import { ServiceApi } from 'src/app/api/ServiceApi';
import { Customer } from './models';
import { Observable } from 'rxjs';
import { ApiRequest } from 'src/app/common/rest-client/ApiRequest';
import { map } from 'rxjs/operators';

@Injectable()
export class MongoDbService {

    constructor(
        private api: ServiceApi) {
    }

    public addCustomer(customer: Customer): Observable<Customer> {
        let link = this.api.apiEntry.getLink("add-customer");
        let request = ApiRequest.fromLink(link, config => config.withContent(customer));

        return this.api.client.send<Customer>(request).pipe(
            map((result) => result.content)
        );
    }

    public readCustomer(id: string): Observable<Customer>  {
        let link = this.api.apiEntry.getLink("read-customer");
        let request = ApiRequest.fromLink(link, config => config.withRouteValues({customerId: id}));

        return this.api.client.send<Customer>(request).pipe(
            map((result) => result.content)
        );
    }

    public updateCustomer(customer: Customer) {
        let link = this.api.apiEntry.getLink("update-customer");

        let request = ApiRequest.fromLink(link, config => { 
            config.withRouteValues({customerId: customer.customerId});
            config.withContent(customer)
        });

        return this.api.client.send<Customer>(request).pipe(
            map((result) => result.content)
        );
    }

}