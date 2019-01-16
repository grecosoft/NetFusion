export class OrderSubmitted {
    partNumber: string;
    state: string;
    quantity: number;
    neededBy?: Date
}