import { APP_ID } from '../../../app.settings'
import { ApiConnection } from "./connections";
import { Notification } from "./notifications";
import { LoadedResource } from './resources';


const APP_AREA = "REST_VIEWER";

export class ApplicationConfigSettings {

    public static get storeKey(): string {
        return `${APP_ID}:${APP_AREA}:APP`
    }

    public settings: ApplicationSettings = new ApplicationSettings();
}

// Stores information pertaining to connections used to establish
// connections to REST/HAL based API Services.
export class ConnectionConfigSettings {

    public static get storeKey(): string {
        return `${APP_ID}:${APP_AREA}:CONN`
    }

    public lastSelectedConnId: string;
    public connections: ApiConnection[] = [];
}

export class NotificationConfigSettings {

    public static get storeKey(): string {
        return `${APP_ID}:${APP_AREA}:NOTIFY`
    }

    public notifications: Notification[] = [];
}

export class EnvironmentConfigSettings {

    public static get storeKey(): string {
        return `${APP_ID}:${APP_AREA}:ENV`
    }
}

export class LoadedResourcesConfigSettings {

    public static get storeKey(): string {
        return `${APP_ID}:${APP_AREA}:CONN_RES`
    }

    public constructor(
        public resources: {[connId: string]: LoadedResource[]}) {
    
    }
}

export class ApplicationSettings {
    
    public saveOpenedTabs: boolean = true;
    public monitorForAuthToken: boolean = false;
    public darkTheme: boolean = true;
}

