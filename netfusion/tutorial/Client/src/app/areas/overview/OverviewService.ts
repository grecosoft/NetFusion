import { Injectable } from '@angular/core';
import { ServiceApi } from 'src/app/api/ServiceApi';
import { Observable } from 'rxjs';
import { ApiRequest } from 'src/app/common/rest-client/ApiRequest';
import { map } from 'rxjs/operators';

@Injectable()
export class OverviewService {

    constructor(
        private api: ServiceApi) {

    }

    public readCompositeLog(): Observable<{}> {
        let link = this.api.apiEntry.getLink("composite-log");
        return this.api.client.send(ApiRequest.fromLink(link))
            .pipe(
                map((result) => result.content)
            );
    }

}