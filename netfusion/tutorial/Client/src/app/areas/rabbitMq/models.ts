export class PropertySold {
    address: string;
    city: string;
    state: string;
    zip: string;
    askingPrice: number;
    soldPrice: number;
}

export class AutoSaleCompleted {
    make: string;
    model: string;
    year: number;
    color: string;
    isNew: boolean;
}

export class TemperatureReading {
    zip: string;
    reading: number;
    coordinates: Coordinates;
}

export class Coordinates {
    north: number;
    west: number;
}

export class SendEmail {
    subject: string;
    formAddress: string;
    toAddresses: string[];
    message: string;
}

export class CalculateAutoTax {
    vin: string;
    zipCode: string;
}

export class CalculatePropertyTax {
    address: string;
    city: string;
    state: string;
    zip: string;
}

export class TaxCalc {
    amount: string;
    dateCalculated: Date
}