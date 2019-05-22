import { NgModule } from '@angular/core';
import { SettingsComponent } from './core/settings/settings.component/settings.component';
import { AreaCommonModule } from './area-common.module';
import { Routes, RouterModule } from '@angular/router';
import { SettingsService } from './core/settings/SettingsService';
import { JsonPipe } from '@angular/common';
import { GitHubService } from './github/GitHubService';
import { CodeViewerComponent } from './github/code-viewer/code-viewer.component';
import { CompositeLogComponent } from './core/bootstrapping/composite-log/composite-log.component';
import { AttributedEntityComponent } from './common/attributed-entity.component/attributed-entity.component';
import { EntityExpressionsComponent } from './common/entity-expressions.component/entity-expressions.component';
import { CommandsComponent } from './core/messaging/commands.component/commands.component';
import { DomainEventsComponent } from './core/messaging/domain-events.component/domain-events.component';
import { QueriesComponent } from './core/messaging/queries.component/queries.component';
import { EnrichersComponent } from './core/messaging/enrichers/enrichers.component';
import { MessagingService } from './core/messaging/MessagingService';
import { RulesComponent } from './core/messaging/rules/rules.component';
import { PublishersComponent } from './core/messaging/publishers/publisher.component';
import { CompositeBuilderComponent } from './core/bootstrapping/composite-builder/composite-builder.component';
import { CompositeApplicationComponent } from './core/bootstrapping/composite-application/composite-application.component';
import { MappingComponent } from './common/mapping-component/mapping.component';
import { ValidationComponent } from './common/validation.component/validation.component';

const areaRoutes: Routes = [
    

    { path: 'settings', component: SettingsComponent },


    { path: 'bootstrapping/composite-application', component: CompositeApplicationComponent },
    { path: 'bootstrapping/composite-builder', component: CompositeBuilderComponent },
    { path: 'bootstrapping/composite-log', component: CompositeLogComponent },

    { path: 'base/attrib-entity', component: AttributedEntityComponent },
    { path: 'base/entity-exp', component: EntityExpressionsComponent },
    { path: 'base/mapping', component: MappingComponent },
    { path: 'base/validation', component: ValidationComponent },

    { path: 'messaging/commands', component: CommandsComponent },
    { path: 'messaging/domain-events', component: DomainEventsComponent },
    { path: 'messaging/queries', component: QueriesComponent },
    { path: 'messaging/enrichers', component: EnrichersComponent },
    { path: 'messaging/rules', component: EnrichersComponent },
    { path: 'messaging/publishers', component: EnrichersComponent },

    
  ];

@NgModule({
    imports: [  
        AreaCommonModule,
        RouterModule.forChild(
            areaRoutes
          )
    ],
    declarations: [
        CompositeLogComponent,

        CodeViewerComponent,

        CompositeBuilderComponent,
        CompositeApplicationComponent,
        MappingComponent,
        ValidationComponent,

        AttributedEntityComponent,
        EntityExpressionsComponent,
        RulesComponent,

        CommandsComponent,
        DomainEventsComponent,
        QueriesComponent,
        EnrichersComponent,
        PublishersComponent,
        SettingsComponent,
  
    ],
    entryComponents: [
        AttributedEntityComponent,
        MappingComponent,
        ValidationComponent,


        CompositeApplicationComponent,
        CompositeBuilderComponent,
        CompositeLogComponent,
        SettingsComponent,

        CommandsComponent,
        DomainEventsComponent,
        QueriesComponent,
        EnrichersComponent,
        RulesComponent,
        PublishersComponent,


        
        EntityExpressionsComponent,

        


       


 

       
    ],
    providers: [
        JsonPipe,
        SettingsService,
  
        GitHubService,

        MessagingService
    ]
})
export class ExamplesModule {

}