import * as _ from 'lodash';
import { FormControl } from '@angular/forms';

import { IHalResource, Link } from '../../../common/client/Resource';
import { ResponseNotification } from './notifications';

export class ResourceLinkViewModel {

    public constructor(
        private name: string,
        private link: Link) {

     }

     public get relName(): string {
        return this.name;
     }

     public get resourceUrl(): string {
        return this.link.href;
     }

     public get isTemplate(): boolean {
         return this.link.templated;
     }

     public get method(): string {
         return this.link.methods[0];
     }

     public get associatedLink(): Link {
         return this.link;
     }

     public get hasContentBody(): boolean {
         return this.link.methods[0] === "POST" || this.link.methods[0] === "PUT";
     }
}

export class ParamValue {
    public constructor(
        public paramName: string,
        public paramInput: FormControl) {

    }
}

export class SelectedResourceLink {

    public content: any;

    public constructor(
        public loadedResource: LoadedResource,
        public link: ResourceLinkViewModel, 
        public linkParams: {[name:string]:any}) {
    }
}

export class LoadedResource {
    
    public links: { [id: string]: Link };
    public embedded: { [id:string]: LoadedResource };
    public lastResponse: IHalResource;
    public lastNotifications: ResponseNotification[] = [];
    
    public constructor(
        public resource: IHalResource,
        public relationName?: string,
        public relationTag?: string) {

        this.links = resource._links;
        this.embedded =  this.loadEmbeddedResources(resource);

        delete resource._links;
        delete resource._embedded;
    }

    public setLastResponse(response: IHalResource) {
        this.lastResponse = response;
    }

    public addLastNotification(notification: ResponseNotification) {
        this.lastNotifications.push(notification);
    }

    private loadEmbeddedResources(resource: IHalResource): {[id:string]: LoadedResource} {

        let embedded = {};

        _.forOwn(resource._embedded, (res, name) => {
            if (_.isArray(res)) {
                let untypedRes = <any>res;
                let resColl = <IHalResource[]>untypedRes;

                let loadedResources = _.map(resColl, r => {
                    return new LoadedResource(r, name);
                });

                embedded[name] = loadedResources;
                
            } else {
                embedded[name] = new LoadedResource(res, name);
            }
        });

        return embedded;
    }
}


