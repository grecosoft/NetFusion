import { NgModule } from "@angular/core";

// Common Anguler UI Components:
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatRadioModule } from '@angular/material/radio';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatListModule } from '@angular/material/list';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialogModule } from '@angular/material/dialog';
import { MatCardModule } from '@angular/material/card';
import { MatTabsModule } from '@angular/material/tabs';
import { MatToolbarModule } from '@angular/material/toolbar';

// Data table support:
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';

// Common application icon support:
import { MatIconModule } from '@angular/material/icon';
import { DomSanitizer } from '@angular/platform-browser';
import { MatIconRegistry, MatButtonToggleModule, MatSnackBarModule } from '@angular/material';

// Flex Layout Components:
import {FlexLayoutModule} from "@angular/flex-layout";

// Imported and Reexported Modules:
const materialComponents = [
    MatButtonModule,
    MatCheckboxModule,
    MatDatepickerModule,
    MatFormFieldModule,
    MatInputModule,
    MatRadioModule,
    MatSelectModule,
    MatSlideToggleModule,
    MatListModule,
    MatTooltipModule,
    MatDialogModule,
    MatIconModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatButtonToggleModule,
    MatCardModule,
    MatTabsModule,
    MatToolbarModule,
    MatSnackBarModule,
    FlexLayoutModule
];

@NgModule({
    imports: materialComponents,
    exports: materialComponents
})
export class MaterialModule {
    constructor(
        private iconRegistry: MatIconRegistry, private sanitizer: DomSanitizer) {

            this.registerIcons([
                'plugin',
                'topic',
                'doc'
            ]);
    }

    registerIcons(icons :string[]) {

        for (let icon of icons) {

            this.iconRegistry.addSvgIcon(
                icon, 
                this.sanitizer.bypassSecurityTrustResourceUrl(`assets/icons/${icon}.svg`));
        }
    }
}