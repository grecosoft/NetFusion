import { Application } from '../Application';

export interface ILoginStatus {
    isUserLoggedIn: boolean;
}

// Base view model that subscribes to the application login notifications
// and contains properties common for all view models.
export class BaseAreaViewModel implements ILoginStatus {
    
    public isUserLoggedIn: boolean;

    public constructor(application: Application) {
        application.whenLoginStateChange.subscribe((value) => {
            this.isUserLoggedIn = value;
        });
    }
}

