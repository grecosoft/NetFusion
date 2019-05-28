import { Component } from '@angular/core';
import { RestService } from '../RestService';
import { ApiResponse } from 'src/app/common/rest-client/ApiResponse';

@Component({
    templateUrl: './resource-linking.component.html',
    styleUrls: ['../../../area.scss']
})
export class ResourceLinkingComponent {

    public linkedResourceResult: ApiResponse<void>;
    public relatedResourceResult: ApiResponse<void>;
    
    public constructor(
        private restService: RestService) {

    }

    public onQueryLinkedResource() {
        this.restService.loadSchool(1).subscribe(result => this.linkedResourceResult = result);
    }

    public onQueryRelatedResource() {
        this.restService.loadStudents(1).subscribe(result => this.relatedResourceResult = result);
    }
}