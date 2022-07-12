import { ChangeDetectorRef, Component, ViewChild } from '@angular/core';
import { FormGroup, NgForm } from '@angular/forms';
import {
  IButtonGroupEventArgs,
  IgxFilterOptions,
  IgxGridComponent,
  IgxStepperComponent,
  IRowSelectionEventArgs,
  IStepChangedEventArgs,
} from '@infragistics/igniteui-angular';
import { NodeObjectTypeDto } from 'src/app/models/Neo4JGraphNode';
import { SAPKeyNodeDto, SAPKeyNodeTableLoadDto } from 'src/app/models/PrimaryKeyDto';
import { Neo4JNodeDto, SAPTableNodeDto } from 'src/app/models/RelevantTableGraphDto';
import { ExtractorAPIService } from 'src/app/services/extractorAPI.service';
import { athletesData } from 'src/assets/TestData/athletesData';
import { ExtractionStep1Component } from '../step-1/extractionstep1.component';
import { ExtractionStep2Component } from '../step-2/extractionstep2.component';
import { ExtractionStep3Component } from '../step-3/extractionstep3.component';

@Component({
  selector: 'app-extractionstep0',
  templateUrl: './extractionstep0.component.html',
  styleUrls: ['./extractionstep0.component.scss'],
})
export class ExtractionStep0Component {
  @ViewChild('grid1', { static: true })
  public grid!: IgxGridComponent;

  @ViewChild('extractionstep1', { static: true })
  public step1Component!: ExtractionStep1Component;

  @ViewChild('extractionstep2', { static: true })
  public step2Component!: ExtractionStep2Component;

  @ViewChild('extractionstep3', { static: true })
  public step3Component!: ExtractionStep3Component;

  @ViewChild('stepper', { static: true })
  public stepper!: IgxStepperComponent;

  public data!: NodeObjectTypeDto[];

  public searchText = '';
  public caseSensitive = false;
  public exactMatch = false;

  public isValid = false;
  public isValidStep2 = false;
  public isValidStep3 = false;
  public isValidStep5 = false;

  constructor(private extractorApiService: ExtractorAPIService) {}

  public ngOnInit() {
    // this.data = athletesData;

    this.extractorApiService.GetObjectTypes().then((result) => {
      console.log(result);
      const objectTypes: NodeObjectTypeDto[] = result;

      this.data = objectTypes;
    });
  }

  public HandleNextStep() {
    this.stepper.next();

    console.log(this.stepper);
  }

  public handleActiveStepChanged(event: IStepChangedEventArgs) {
    console.log(event);
    const selectedRow : NodeObjectTypeDto = this.grid.selectedRows[0];


    switch (event.index) {
      case 0:
        break;
      case 1:
        console.log('Entering Step 2: ObjectType Name: ' + selectedRow.Name);

        this.loadStep1(selectedRow.Name);
        break;
      case 2:
        console.log('Entering Step 3: ObjectType Name: ' + selectedRow.Name);

        this.loadStep2(selectedRow.Name);

        this.isValidStep3 = true;
        break;
      case 3:
        console.log('Entering Step 4: ObjectType Name: ' + selectedRow.Name);

        this.loadStep3();
      break;
      case 4:
        console.log('Entering Step 5: ObjectType Name: ' + selectedRow.Name);

        const selectedPrimaryKeys = this.step3Component.GetSelectedPrimaryKeys();
        const selectedTables = this.step2Component.GetSelectedTables();

        this.PerformExtraction(selectedTables, selectedPrimaryKeys);
      break;

      default:
        break;
    }
  }

  private loadStep1(objectTypeName: string) {
    this.step1Component.GetObjectTypeNetwork(objectTypeName);
  }

  private loadStep2(objectTypeName: string) {
    this.step2Component.GetObjectTypeNetwork(objectTypeName);
  }

  private loadStep3() {
    
    // List for the Primary Keys that need to be loaded.
    let primaryKeys: SAPKeyNodeDto[] = [];

    // Get the seleceted Tables from step 2
    const selectedTables = this.step2Component.GetSelectedTables();

    selectedTables.forEach(table => {
      table.PrimaryKeyNodes.forEach(primaryKey => {
        // console.log(primaryKey);
        const found = primaryKeys.filter(x => x.Name === primaryKey.Name);
        // console.log(found);
        if(found.length === 0) {
          primaryKeys.push(primaryKey);
        }
      });
    });

    primaryKeys = primaryKeys.sort((a, b) => (a.Name < b.Name ? -1 : 1));
    console.log(primaryKeys);
    this.step3Component.LoadPrimaryKeyList(primaryKeys, this.step2Component.Tables);
  }


  private PerformExtraction(selectedTables: SAPTableNodeDto[], selectedPrimaryKeys: SAPKeyNodeTableLoadDto[]) {
    
    this.extractorApiService.PerformExtraction(selectedTables, selectedPrimaryKeys).then((result) => {
      console.log(result);
    });
  }

  public handleRowSelection(event: IRowSelectionEventArgs) {
    console.log(event);
    const targetCell = event.newSelection;

    if (event.newSelection.length > 0) {
      this.isValid = true;
    } else {
      this.isValid = false;
    }
  }

  // Search Relevant Code

  public clearSearch() {
    this.searchText = '';
    this.grid.clearSearch();
  }

  public searchKeyDown(ev: any) {
    if (
      ev.key === 'Enter' ||
      ev.key === 'ArrowDown' ||
      ev.key === 'ArrowRight'
    ) {
      ev.preventDefault();
      this.grid.findNext(this.searchText, this.caseSensitive, this.exactMatch);
    } else if (ev.key === 'ArrowUp' || ev.key === 'ArrowLeft') {
      ev.preventDefault();
      this.grid.findPrev(this.searchText, this.caseSensitive, this.exactMatch);
    }
  }

  public updateSearch() {
    this.caseSensitive = !this.caseSensitive;
    this.grid.findNext(this.searchText, this.caseSensitive, this.exactMatch);
  }

  public updateExactSearch() {
    this.exactMatch = !this.exactMatch;
    this.grid.findNext(this.searchText, this.caseSensitive, this.exactMatch);
  }
}
