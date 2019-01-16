import { Inject, Injectable } from "@angular/core";
import { StorageService, LOCAL_STORAGE } from "angular-webstorage-service";

// Wrapper service around the local storage library being used.
@Injectable()
export class LocalStorageService {

    constructor(
        @Inject(LOCAL_STORAGE) private storage: StorageService) { 
    }

    public save(key: string, value: any) {
        this.storage.set(key, value);
    }

    public load<T>(key: string): T {
        return <T>this.storage.get(key);
    }

    public remove(key: string) {
        return this.storage.remove(key);
    }
}