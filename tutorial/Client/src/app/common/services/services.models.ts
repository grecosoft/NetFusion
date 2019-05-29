import { IHalResource } from '../rest-client/Resources';

// Indicates the level of importance associated with a notification.
export enum NotificationLevel {
    Information = 0,
    Warning = 1,
    Error = 2
}

export abstract class Notification {
    protected _level: NotificationLevel;

    public get level(): NotificationLevel {
        return this._level;
    }
}

// A derived notification that is published to notify other 
// components/services of a change in application state.
export class Alert extends Notification {
    private _message: string;

    public get message(): string
    {
        return this._message;
    }

    public static create(message: string, level: NotificationLevel): Alert {
        let instance = new Alert();
        instance._level = level;
        instance._message = message;

        return instance;
    }
}

// A derived notification that is published to notify other
// components/services of a change in resource state.
export class ResourceUpdate extends Notification {
    private _id: string;
    private _resource: IHalResource;
    private _typeIdentifier: string;

    public get id(): string {
        return this._id;
    }

    public get resource(): IHalResource
    {
        return this._resource;
    }

    public get typeIdentifier(): string
    {
        return this._typeIdentifier;
    }

    public static create(id: string, resource: IHalResource, typeIdentifier: string = null) : ResourceUpdate {
        let instance = new ResourceUpdate()
        instance._id = id;
        instance._resource = resource;
        instance._typeIdentifier = typeIdentifier;
        
        return instance;
    }
}

export class ConfirmationInfo {
    public constructor(
        public message: string,
        public actionName: string) {
            
        }
}
