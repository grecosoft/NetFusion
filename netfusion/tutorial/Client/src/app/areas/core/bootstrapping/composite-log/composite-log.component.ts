import { Component, OnInit } from '@angular/core';
import { OverviewService } from '../OverviewService';

@Component({
    templateUrl: './composite-log.component.html',
    styleUrls: ['../../../area.scss'] 
})
export class CompositeLogComponent implements OnInit {

    public compositeLog: any;

    constructor(
        private overviewService: OverviewService) {

    }

    ngOnInit(): void {
        this.compositeLog = this.overviewService.readCompositeLog()
            .subscribe((log) => {
                this.compositeLog = log;
            });
    }
}
