import { Component, Input } from '@angular/core';

@Component({
    selector: 'method-indicator',
    styleUrls: ['./method-indicator.component.scss'],
    templateUrl: './method-indicator.component.html'    
})
export class MethodIndicatorComponent {

    @Input('method')
    public method: string;

    public get methodClass(): string {
        return 'http-' + this.method.toLowerCase();
    }
} 