import { Component } from '@angular/core';
import { Customer } from '../models';
import { MongoDbService } from '../MongoDbService';
import { JsonPipe } from '@angular/common';

@Component({
    templateUrl: './mongoDb.component.html',
    styleUrls: ['../../area.scss']
})
export class MongoDbComponent {

    public nugetPackageName = "NetFusion.MongoDB";
    public documentationUrl = "https://github.com/grecosoft/NetFusion/wiki/integration.mongodb.overview#mongodb-overview";
    public containerScope = "Singleton";

    public newCustomer: Customer;
    public newCustomerBody: string;
    public newCustomerResult: Customer;

    public readCustomerId: string;
    public readCustomerResult: Customer;

    public updatedCustomerBody: string;
    public updatedCustomerResult: Customer;

    constructor(
        private json: JsonPipe,
        private mongoDbService: MongoDbService
    ){

        this.newCustomer = {
            firstName: "Tom",
            lastName: "Green",
            age: 65,
            companyName: "Something Inc.",
            jobTitle: "Sr. Know it all.",
            addresses: [
                { 
                    addressLine1: "111 West Main street",
                    city: "Cheshire",
                    state: "CT",
                    zip: "06410"
                }
            ]
        };

        this.newCustomerBody = this.json.transform(this.newCustomer);
    }

    public addEntity() {
        let customer = <Customer>JSON.parse(this.newCustomerBody);

        this.mongoDbService.addCustomer(customer)
            .subscribe((result) => {
                this.newCustomerResult = result;
            });
    }

    public readEntity() {
        this.mongoDbService.readCustomer(this.readCustomerId)
            .subscribe((result) => {
                this.readCustomerResult = result;
                this.updatedCustomerBody = this.json.transform(result);
            });
    }

    public updateEntity() {
        let customer = <Customer>JSON.parse(this.updatedCustomerBody);

        this.mongoDbService.updateCustomer(customer)
            .subscribe((result) => {
                this.updatedCustomerResult = result;
            });
    }

}