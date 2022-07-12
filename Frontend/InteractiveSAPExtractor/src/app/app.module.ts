import { APP_INITIALIZER, NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BrowserModule, HammerModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import {
  IgxAutocompleteModule,
  IgxAvatarModule,
  IgxBadgeModule,
  IgxButtonGroupModule,
  IgxButtonModule,
  IgxCardModule,
  IgxCheckboxModule,
  IgxChipsModule,
  IgxDialogModule,
  IgxFilterModule,
  IgxGridModule,
  IgxHierarchicalGridModule,
  IgxIconModule,
  IgxInputGroupModule,
  IgxLayoutModule, IgxListModule, IgxMaskModule, IgxNavbarModule, IgxNavigationDrawerModule, IgxProgressBarModule, IgxRadioModule, IgxRippleModule, IgxSelectModule, IgxStepperModule, IgxSwitchModule, IgxTabsModule
} from '@infragistics/igniteui-angular';

import {MatButtonModule} from '@angular/material/button';
import {MatIconModule} from '@angular/material/icon';
import {MatProgressBarModule} from '@angular/material/progress-bar';



import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { AuthenticationModule, ExternalAuthService } from './authentication';
import { GraphViewerComponent } from './graphViewer/graphViewer.component';
import { HomeComponent } from './home/home.component';

import { ExtractionStep0Component } from './sap-extraction/step-0-Home/extractionstep0.component';
import { ExtractionStep1Component } from './sap-extraction/step-1/extractionstep1.component';
import { ExtractionStep2Component } from './sap-extraction/step-2/extractionstep2.component';
import { AutocompletePipeStartsWith, ExtractionStep3Component } from './sap-extraction/step-3/extractionstep3.component';
import { AppConfig } from './base/app-config';
import { Logger } from './base/logger';
import { ExtractorAPIService } from './services/extractorAPI.service';
import { VisNetworkHelper } from './helper/VisNetworkHelper';
import { GraphViewerStep0Component } from './graphViewer/graphViewer-Step0/graphViewerStep0.component';

export function loadContext(config: AppConfig) {
  return () => config.load();
}

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    GraphViewerComponent,

    GraphViewerStep0Component,

    ExtractionStep0Component,
    ExtractionStep1Component,
    ExtractionStep2Component,
    ExtractionStep3Component,

    AutocompletePipeStartsWith
  ],
  imports: [
    FormsModule,
    BrowserModule,
    HammerModule,
    BrowserAnimationsModule,
    // NOTE: `AuthenticationModule` defines child routes, must be imported before root `AppRoutingModule`
    AuthenticationModule,
    AppRoutingModule,


    IgxAvatarModule,
    IgxAutocompleteModule,
    IgxNavigationDrawerModule,
    IgxNavbarModule,
    IgxLayoutModule,
    IgxRippleModule,
    IgxDialogModule,
    IgxStepperModule,
    IgxMaskModule,
    IgxInputGroupModule,
    IgxButtonModule,
    IgxRadioModule,
    IgxCardModule,
    IgxCheckboxModule,
    IgxSelectModule,
    IgxIconModule,
    IgxBadgeModule,
    IgxButtonGroupModule,
    IgxFilterModule,
    IgxListModule,
    IgxGridModule,
    IgxChipsModule,
    IgxTabsModule,
    IgxHierarchicalGridModule,
    IgxProgressBarModule,
    IgxSwitchModule,


    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
  ],
  providers: [
    AppConfig,
    {
      provide: APP_INITIALIZER,
      useFactory: loadContext,
      deps: [AppConfig],
      multi: true
    },
    Logger,
    ExtractorAPIService,
    VisNetworkHelper

  ],
  bootstrap: [AppComponent]
})
export class AppModule {

  constructor(private externalAuthService: ExternalAuthService) {
    /**
     * To register a social login, un-comment one or more of the following and add your service provider Client ID.
     * See https://github.com/IgniteUI/igniteui-cli/wiki/Angular-Authentication-Project-Template#add-a-third-party-social-provider
     */
    // this.externalAuthService.addGoogle('<CLIENT_ID>');

    // this.externalAuthService.addMicrosoft('<CLIENT_ID>');

    // this.externalAuthService.addFacebook('<CLIENT_ID>');
  }

}
