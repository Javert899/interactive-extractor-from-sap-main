import {
  AfterViewInit,
  Component,
  ElementRef,
  OnInit,
  ViewChild,
  ViewContainerRef,
} from '@angular/core';
import {
  IgxDialogComponent,
  IgxDropDownComponent,
  IgxGridComponent,
  IgxStepperComponent,
  IRowSelectionEventArgs,
  IStepChangedEventArgs,
} from '@infragistics/igniteui-angular';
import { SAPKeyNodeDto } from 'src/app/models/PrimaryKeyDto';
import { ExtractionStep1Component } from 'src/app/sap-extraction/step-1/extractionstep1.component';
import { ExtractionStep2Component } from 'src/app/sap-extraction/step-2/extractionstep2.component';
import { ExtractionStep3Component } from 'src/app/sap-extraction/step-3/extractionstep3.component';
import * as vis from 'vis';
import { NodeObjectTypeDto } from '../../models/Neo4JGraphNode';
import { ExtractorAPIService } from '../../services/extractorAPI.service';

@Component({
  selector: 'app-graphviewerStep0',
  templateUrl: './graphViewerStep0.component.html',
  styleUrls: ['./graphViewerStep0.component.scss'],
})
export class GraphViewerStep0Component implements OnInit {
  @ViewChild('grid1', { static: true })
  public grid!: IgxGridComponent;

  @ViewChild('graphView1', { static: true })
  public graphView1!: ExtractionStep1Component;

  @ViewChild('stepper', { static: true })
  public stepper!: IgxStepperComponent;

  public data!: NodeObjectTypeDto[];

  public searchText = '';
  public caseSensitive = false;
  public exactMatch = false;

  public isValid = false;

  constructor(private extractorApiService: ExtractorAPIService) {}

  public ngOnInit() {
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
        console.log('Entering Graph View 2: ObjectType Name: ' + selectedRow.Name);

        this.loadView1(selectedRow.Name);
        break;

      default:
        break;
    }
  }

  private loadView1(objectTypeName: string) {
    this.graphView1.GetObjectTypeNetwork(objectTypeName);
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
