import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ServiceApi } from '../../api/ServiceApi';
import { environment } from 'src/environments/environment.prod';


@Injectable()
export class GitHubService {
                        
    constructor(
        private api: ServiceApi) {
    }

    public getNetFusionSrc(fileName: string): Observable<string> {
        let url = `${environment.netFusionCodeRootPath}/${fileName}`;
        return this.api.client.http.get(url, {responseType: 'text'});
    }

    public getTutorialWebApiSrc(fileName: string): Observable<string> {
        let url = `${environment.tutorialCodeRootPath}/Service/src/Service.WebApi/${fileName}`;
        return this.api.client.http.get(url, {responseType: 'text'});
    }

    public getTutorialComponentSrc(fileName: string): Observable<string> {
        let url= `${environment.tutorialCodeRootPath}/Service/src/Components/${fileName}`;
        return this.api.client.http.get(url, {responseType: 'text'});
    }

    public getTutorialClientSrc(fileName: string): Observable<string> {
        let url = `${environment.tutorialCodeRootPath}/Service/src/Service.Client/${fileName}`;
        return this.api.client.http.get(url, {responseType: 'text'});
    }
} 


