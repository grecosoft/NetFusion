import { Component } from '@angular/core';
import { Application } from '../../models/Application';
import { ApiConnection } from '../../models/connections';

@Component({
    templateUrl: './area-toolbar.component.html',
    styleUrls: ['./area-toolbar.component.scss']
})
export class AreaToolbarComponent {

    public constructor(
        private application: Application) {

    }

    public get connections(): ApiConnection []{
        return this.application.connSettings.connections;
    }

    public get selectedConn(): ApiConnection {
        return this.application.selectedConn;
    }

    public connSelected(connection: ApiConnection) {
        this.application.setSelectedConnection(connection);
    }
}