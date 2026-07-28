import { Injectable } from "@angular/core";
import { ConfigData } from "./models/config-model";
import { HttpClient } from "@angular/common/http";

@Injectable({providedIn: 'root'})
export class ConfigDataService {

    configData!: ConfigData;

    constructor(private http: HttpClient) { }

    load() {
        return this.http.get<ConfigData>("assets/config.json")
                        .toPromise()
                        .then(c => { 
                            if (c != undefined) {
                                this.configData = c
                            }
                        });
    }
}