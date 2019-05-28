import { Component } from '@angular/core';
import { ApiResponse } from 'src/app/common/rest-client/ApiResponse';
import { JsonPipe } from '@angular/common';
import { RoslynService } from './RoslynService';

@Component({
    templateUrl: './roslyn-expressions.component.html',
    styleUrls: ['../../area.scss']
})
export class RoslynExpressionsComponent {

    public newEntityBody: string;
    public evaluatedEntityResult: ApiResponse<void>;

    public constructor(
        private json: JsonPipe,
        private roslynService: RoslynService
    ) {
        let entity: any = {
            "displayName": "Living Room",
            "currentValue": 70,
            "isActiveAlert": false,
            "attributeValues": {
                "MinTemp": 65,
                "MaxTemp": 82,
                "DesiredTemp": 72,
                "FreezingTemp": 50,
                "IsFreezing": false,
                "OfficeClosed": false,
                "SomeCalc": 0.63331920308629985,
                "AnotherCalc": "yes"
            }
        };

        this.newEntityBody = this.json.transform(entity)
    }

    public onEvaluateEntity() {
        let entity = <any>JSON.parse(this.newEntityBody);

        this.roslynService.applyExpressionsToEntity(entity)
            .subscribe(result => this.evaluatedEntityResult = result);
    }
}