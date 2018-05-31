import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { RequestClientFactory } from './RequestClientFactory';

@NgModule({
    imports: [
        CommonModule,
        HttpClientModule
    ],
    providers: [RequestClientFactory]
})
export class ClientModule {

}

 