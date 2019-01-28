import { Injectable } from '@angular/core';
import { ServiceApi } from 'src/app/api/ServiceApi';
import { Observable } from 'rxjs';
import { CalculationSettings } from './models';
import { ApiRequest } from 'src/app/common/rest-client/ApiRequest';
import { map } from 'rxjs/operators';

@Injectable()
export class SettingsService {

    constructor(
        private api: ServiceApi) {

    }

    public readSettings(): Observable<CalculationSettings> {
        let link = this.api.apiEntry.getLink("injected-settings");
        return this.api.client.send(ApiRequest.fromLink(link))
            .pipe(
                map((result) => result.content)
            );
    }
}