import { Input, Component } from '@angular/core';
import { GitHubService } from '../GitHubService';
import { Observable } from 'rxjs';
import { DialogService } from 'src/app/common/dialogs/DialogService';
import { CodeDialogComponent } from './code-dialog.component';

@Component({
    selector: 'code-viewer',
    template: `<a (click)="onViewSource()"><ng-content></ng-content></a>`,
    styleUrls: ['./code-viewer.component.scss']
})
export class CodeViewerComponent {

    private _typeMethods: {[key:string]:(name: string)=>Observable<string>} = {};

    constructor(
        private gitHubService: GitHubService,
        private dialogService: DialogService) {
        
        this.setMethodTypes();
    }

    @Input()
    public type: string;
    
    @Input()
    public name: string;

    public onViewSource() {
        this.getSource().subscribe((code)=> {
            
            console.log(code);

            this.dialogService.openDialog(CodeDialogComponent, 
                {
                    code: code
                }, 
                { width: '950px', minWidth: '950px', height: '700px', minHeight: '700px'});

        });
    }

    private getSource(): Observable<string> {
        let srcFn = this._typeMethods[this.type].bind(this.gitHubService);
        return srcFn(this.name);
    }

    private setMethodTypes() {
        this._typeMethods['net-fusion'] = this.gitHubService.getNetFusionSrc;
        this._typeMethods['tutorial-components'] = this.gitHubService.getTutorialComponentSrc;
        this._typeMethods['tutorial-webapi'] = this.gitHubService.getTutorialWebApiSrc;
        this._typeMethods['tutorial-client'] = this.gitHubService.getTutorialClientSrc;
    }
}