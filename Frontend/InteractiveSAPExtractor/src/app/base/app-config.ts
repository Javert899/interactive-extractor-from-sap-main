import { Inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/take';
import { catchError } from 'rxjs/operators';
import { Logger } from './logger';
import {Observable} from "rxjs"




@Injectable()
export class AppConfig {

    private config: any = null;
    private env: any = null;

    constructor(private http: HttpClient) {

    }

    /**
     * Use to get the data found in the second file (config file)
     */
    public getConfig(key: string) {
        return this.config[key];
    }

    /**
     * Use to get the data found in the first file (env file)
     */
    public getEnv(key: any) {
        return this.env[key];
    }

    //
    // wenn man schon mal http hat, dies auch nutzbar machen...
    //
    public getHttp() {
        return this.http;
    }

    /**
     * This method:
     *   a) Loads "env.json" to get the current working environment (e.g.: 'production', 'development')
     *   b) Loads "config.[env].json" to get all env's variables (e.g.: 'config.development.json')
     */
    public load() {
         return new Promise((resolve, reject) => {
            this.http.get('./assets/env.json').subscribe((envResponse) => {
                const envTemp: any = envResponse;
                this.env = envTemp;
                let request: any = null;

                switch (envTemp.env) {
                    case 'production': {
                        request = this.http.get('./assets/config.' + envTemp.env + '.json');
                    } break;

                    case 'development': {
                        request = this.http.get('./assets/config.' + envTemp.env + '.json');
                    } break;

                    case 'default': {
                        console.error('Environment file is not set or invalid');
                        resolve(true);
                    } break;
                }

                if (request) {
                    request
                        .subscribe((responseData: any) => {
                            this.config = responseData;

                            resolve(true);
                        });
                } else {
                    console.error('Env config file "env.json" is not valid');
                    resolve(true);
                }
            });

        });
    }
}
