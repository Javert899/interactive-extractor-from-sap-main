import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { ExtractorAPIService } from '../services/extractorAPI.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent  {
  title = 'Welcome to Interactive Extractor for SAP ERP!';
  constructor(private router: Router, private extractorApiService: ExtractorAPIService) 
  { 

  }


  StartExtraction() {
    // Navigate to the step-by-step extractor
    this.router.navigate(['/extraction'], {
      queryParams: {
      }
    });    
  }
}
