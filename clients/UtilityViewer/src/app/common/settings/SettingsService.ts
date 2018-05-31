import { Injectable } from '@angular/core';
import { PersistenceService, StorageType } from 'angular-persistence';

// Service used to save application settings.
@Injectable()
export class SettingsService {

    public constructor(
        private persistenceService: PersistenceService) {
    }

    public read<TSettings>(key: string) {
        return <TSettings>this.persistenceService.get(key, StorageType.LOCAL);
    }

    public save(key: string, settings: any) {
        this.persistenceService.set(key, settings, { type: StorageType.LOCAL });
    }
}