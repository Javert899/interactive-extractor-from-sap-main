import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';

import { PageNotFoundComponent } from './error-routing/not-found/not-found.component';
import { UncaughtErrorComponent } from './error-routing/error/uncaught-error.component';
import { ErrorRoutingModule } from './error-routing/error-routing.module';
import { GraphViewerComponent } from './graphViewer/graphViewer.component';
import { ExtractionStep0Component } from './sap-extraction/step-0-Home/extractionstep0.component';
import { ExtractionStep1Component } from './sap-extraction/step-1/extractionstep1.component';
import { ExtractionStep2Component } from './sap-extraction/step-2/extractionstep2.component';
import { GraphViewerStep0Component } from './graphViewer/graphViewer-Step0/graphViewerStep0.component';

export const routes: Routes = [
  { path: '', redirectTo: '/home', pathMatch: 'full'},
  { path: 'home', component: HomeComponent, data: { text: 'Home' }},
  { path: 'graphViewer', component: GraphViewerStep0Component, data: { text: 'Graph Viewer' }},
  { path: 'extraction', component: ExtractionStep0Component, data: { text: 'SAP Extraction' }, children: [
    {
      path: 'step-1', // child route path
      component: ExtractionStep1Component, // child route component that the router renders
    },
    {
      path: 'step-2',
      component: ExtractionStep2Component, // another child route component that the router renders
    },],
  },
    

  
  { path: 'error', component: UncaughtErrorComponent },
  { path: '**', component: PageNotFoundComponent } // must always be last
];

@NgModule({
  imports: [RouterModule.forRoot(routes, { useHash: true }), ErrorRoutingModule],
  exports: [RouterModule, ErrorRoutingModule]
})
export class AppRoutingModule { }
