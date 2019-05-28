import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AreaModule } from '../area.module';
import { JsonPipe } from '@angular/common';
import { CommandsComponent } from './messaging/commands.component/commands.component';
import { DomainEventsComponent } from './messaging/domain-events.component/domain-events.component';
import { QueriesComponent } from './messaging/queries.component/queries.component';
import { EnrichersComponent } from './messaging/enrichers/enrichers.component';
import { RulesComponent } from './messaging/rules/rules.component';
import { PublishersComponent } from './messaging/publishers/publisher.component';
import { MessagingService } from './messaging/MessagingService';
import { CompositeBuilderComponent } from './bootstrapping/composite-builder/composite-builder.component';
import { CompositeApplicationComponent } from './bootstrapping/composite-application/composite-application.component';
import { CompositeLogComponent } from './bootstrapping/composite-log/composite-log.component';
import { SettingsComponent } from './settings/settings.component/settings.component';
import { OverviewService } from './bootstrapping/OverviewService';
import { SettingsService } from './settings/SettingsService';

const areaRoutes: Routes = [
    { path: 'bootstrapping/composite-application', component: CompositeApplicationComponent },
    { path: 'bootstrapping/composite-builder', component: CompositeBuilderComponent },
    { path: 'bootstrapping/composite-log', component: CompositeLogComponent },

    { path: 'settings', component: SettingsComponent },

    { path: 'messaging/commands', component: CommandsComponent },
    { path: 'messaging/domain-events', component: DomainEventsComponent },
    { path: 'messaging/queries', component: QueriesComponent },
    { path: 'messaging/enrichers', component: EnrichersComponent },
    { path: 'messaging/rules', component: RulesComponent },
    { path: 'messaging/publishers', component: PublishersComponent },
];

@NgModule({
    imports: [
        AreaModule,
        RouterModule.forChild(
            areaRoutes
          )
    ],
    declarations: [
        CommandsComponent,
        DomainEventsComponent,
        QueriesComponent,
        EnrichersComponent,
        RulesComponent,
        PublishersComponent,
        CompositeApplicationComponent,
        CompositeBuilderComponent,
        CompositeLogComponent,
        SettingsComponent,
       
    ],
    entryComponents: [
        CommandsComponent,
        DomainEventsComponent,
        QueriesComponent,
        EnrichersComponent,
        RulesComponent,
        PublishersComponent,
        CompositeApplicationComponent,
        CompositeBuilderComponent,
        CompositeLogComponent,
        SettingsComponent
    ],
    providers: [
        JsonPipe,
        MessagingService,
        OverviewService,
        SettingsService
    ]
})
export class CoreAreaModule {
    
}