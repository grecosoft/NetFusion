export class Customer {
    public customerId?: string;
    public firstName: string;
    public lastName: string;
    public age: number;
    public jobTitle: string;
    public companyName: string;
    public addresses: Address[];
}

export class Address {
    public addressLine1: string;
    public addressLine2?: string;
    public city: string;
    public state: string;
    public zip: string;
}