import { Component } from '@angular/core';
import { RedisService } from '../RedisService';


@Component({
    templateUrl: './redis-data.component.html',
    styleUrls: ['../../../area.scss'] 
})
export class RedisDataComponent {

    //public injectedSettings: CalculationSettings;
    public nugetPackageName = "NetFusion.Redis";
    public documentationUrl = "https://github.com/grecosoft/NetFusion/wiki/integration.redis.overview#redis-overview";
    public containerScope = "Singleton";

    public value: string;
    public poppedValue: string;

    constructor(
        private redisService: RedisService) {

    }

    public setValue() {
        this.redisService.setValue(this.value)
            .subscribe();
    }

    public popValue() {
        this.redisService.popValue()
            .subscribe(response => this.poppedValue = response.value);
    }

}