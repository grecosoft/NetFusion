
export class SelectList {

    public items: SelectItem[] = [];

    public addItem(value: any, displayValue: string) {
        this.items.push(new SelectItem(value, displayValue));
        this[value] = displayValue;
    }
}

export class SelectItem {
    public constructor(
        public value: any,
        public displayValue: string) {

    }
}

