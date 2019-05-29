import { Component } from '@angular/core';
import { ApiResponse } from 'src/app/common/rest-client/ApiResponse';
import { AttributedEntityService } from '../AttributedEntityService';
import { JsonPipe } from '@angular/common';

@Component({
    templateUrl: './attributed-entity.component.html',
    styleUrls: ['../../area.scss']
})
export class AttributedEntityComponent {

    public attributedEntityResult: ApiResponse<void>;
    public newAttributesBody: string;
    public updatedAttributedEntityResult: ApiResponse<void>;
    public dynamicAttributeResult: ApiResponse<void>;

    public constructor(
        private json: JsonPipe,
        private commonService: AttributedEntityService) {
        
        let attributes: any = {
            "attributes": [
                {
                    "name": "ValueOne",
                    "value": "Test Value One!"
                },
                {
                    "name": "ValueTwo",
                    "value": "Test Value Two!"
                }
            ]
        };

        this.newAttributesBody = this.json.transform(attributes)
    }

    public onReadAttributedEntity() {
        this.commonService.ReadAttributedEntity()
            .subscribe(result => this.attributedEntityResult = result);
    }

    public onUpdateAttributedEntity() {
        let attributes = <any>JSON.parse(this.newAttributesBody);

        this.commonService.UpdateAttributedEntity(attributes)
            .subscribe(result => this.updatedAttributedEntityResult = result);
    }

    public onReadAttributesDynamically() {
        this.commonService.ReadAttributesDynamically()
            .subscribe(result => this.dynamicAttributeResult = result);
    }
}