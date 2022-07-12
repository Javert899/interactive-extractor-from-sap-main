import {
  Injectable
} from '@angular/core';
import {
  AppConfig
} from '../base/app-config';


@Injectable({
  providedIn: 'root'
})
export class Logger {
  // tslint:disable:max-line-length
  constructor(private localConfig: AppConfig) {

  }

  /* Verbose Levels
  0 - All
  1 - Basic Infos
  2 - Basic Warns
  3 - Critical Warns
  4 - Errors
  5 - Nothing
  */


  //#region Attribute
  globalVerboseLevel = this.localConfig.getConfig('verboseLevel');


  public log(verboselevel: number, message: any) {
    // console.log('Log');
    if (this.globalVerboseLevel <= verboselevel) {
      console.log(message);
    }
  }

  public warn(verboselevel: number, message: any) {
    if (this.globalVerboseLevel <= verboselevel) {
      console.warn(message);
    }
  }

  public error(verboselevel: number, message: any) {
    if (this.globalVerboseLevel <= verboselevel) {
      console.error(message);
    }
  }

  logMessage(infoLevel: number, message: string, messageType: string) {
    if (infoLevel >= this.globalVerboseLevel) {
      switch (message) {
        case 'Info':
          console.log(message);
          break;
        case 'Warn':
          console.warn(message);
          break;
        case 'Error':
          console.error(message);
          break;
      }
    }
  }

}
