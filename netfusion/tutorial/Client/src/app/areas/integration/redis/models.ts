export class OrderSubmitted {
    partNumber: string;
    state: string;
    quantity: number;
    neededBy?: Date
}

export class SetValue {
    value: string;
}