import {
  AfterViewInit,
  Component,
  ElementRef,
  OnInit,
  Pipe,
  PipeTransform,
  ViewChild
} from '@angular/core';
import {
  IgxGridComponent,
  IgxDialogComponent,
  IgxLinearProgressBarComponent,
  IgxFilterOptions,
  IGridEditEventArgs
} from '@infragistics/igniteui-angular';
import {
  IgxGridCellComponent
} from '@infragistics/igniteui-angular/lib/grids/cell.component';
import {
  filter
} from 'jszip';
import {
  SAPKeyNodeDto,
  SAPKeyNodeTableLoadDto
} from 'src/app/models/PrimaryKeyDto';
import {
  Neo4JNodeDto,
  SAPTableNodeDto
} from 'src/app/models/RelevantTableGraphDto';
import {
  ExtractorAPIService
} from 'src/app/services/extractorAPI.service';

@Component({
  selector: 'app-extractionstep3',
  templateUrl: './extractionstep3.component.html',
  styleUrls: ['./extractionstep3.component.scss']
})
export class ExtractionStep3Component implements OnInit, AfterViewInit {
  @ViewChild('grid', {
    static: true
  }) public grid!: IgxGridComponent;

  @ViewChild('dialogFilterValue', {
    read: IgxDialogComponent,
    static: true
  })
  public dialogFilterValue!: IgxDialogComponent;



  @ViewChild('dialogFilterGrid', {
    static: true
  }) public dialogFilterGrid!: IgxGridComponent;



  public tablesData: SAPTableNodeDto[] = [];
  public primaryKeysData: SAPKeyNodeTableLoadDto[] = [];

  public searchText = '';
  public caseSensitive = false;
  public exactMatch = false;

  public autoCompleteValues: any;
  public filterValueSelected = '';
  public currentEditKey!: SAPKeyNodeTableLoadDto;

  constructor(private extractorApiService: ExtractorAPIService) {

  }


  ngAfterViewInit(): void {

  }

  ngOnInit() {

  }

  public LoadPrimaryKeyList(primaryKeys: SAPKeyNodeDto[], tables: SAPTableNodeDto[]) {
    const tempPrimaryKeysData: SAPKeyNodeTableLoadDto[] = [];
    primaryKeys.forEach(key => {
      const currPrimKey: SAPKeyNodeTableLoadDto = {
        KeyNode: key,
        Loading: true,
        FilterValue: ''
      }

      tempPrimaryKeysData.push(currPrimKey);
    })

    this.primaryKeysData = tempPrimaryKeysData;

    this.tablesData = tables;

    this.LoadPrimaryKeyValues();
  }

  private LoadPrimaryKeyValues() {
    this.primaryKeysData.forEach(key => {

      const relevantTables = this.tablesData.filter(x => x.PrimaryKeyNodes.filter(y => y.Name === key.KeyNode.Name).length > 0);
      /*
      console.log(key);
      console.log(relevantTables);
      */
      this.extractorApiService.GetPrimaryKeyValues(key.KeyNode, relevantTables).then((result) => {
        const tableData: SAPKeyNodeTableLoadDto[] = this.grid.data as SAPKeyNodeTableLoadDto[];

        const currentRow = tableData.find(x => x.KeyNode.Id === key.KeyNode.Id);

        if (currentRow != undefined) {
          currentRow.Loading = false;
        }

        key.KeyNode.PossibleValues = result;
      });
    });
  }

  editFilterValue(editGridCell: IgxGridCellComponent) {
    this.dialogFilterValue.open();

    this.autoCompleteValues = [];
    this.dialogFilterGrid.deselectAllRows();


    const rowData = editGridCell.row.data as SAPKeyNodeTableLoadDto;
    
    const correspondingPrimaryKey = this.primaryKeysData.find(x => x.KeyNode.Id == rowData.KeyNode.Id);

    // Refresh the list of possible values.
    if (correspondingPrimaryKey !== undefined) {
      const tempPrimaryKeyValues: {
        Id: string;Value: string;
      } [] = [];
      let currentId = 0;

      // Create the items for the possible values.
      correspondingPrimaryKey.KeyNode.PossibleValues.forEach(x => {
        const tempPrimKey = {
          Id: x,
          Value: x
        };

        tempPrimaryKeyValues.push(tempPrimKey);

        currentId++;
      });

      this.autoCompleteValues = tempPrimaryKeyValues;

   

      const currentSelection = correspondingPrimaryKey.FilterValue.split(',');
      const selectionIds: number[] = [];
      currentSelection.forEach(selectedValue => {
        const foundEntry = this.dialogFilterGrid.getRowByKey(selectedValue);

        if(foundEntry !== undefined) {
          selectionIds.push(foundEntry.index);
        }
      })
      this.dialogFilterGrid.selectRows(selectionIds);


      this.currentEditKey = correspondingPrimaryKey;
    }
  }
  



  public SaveFilterValue() {
    console.log('Dialog closed');
    console.log(this.currentEditKey);

    // Get the selected Rows
    const selectedFilterValues =this.dialogFilterGrid.selectedRows;

    let newFilterVal = '';
    selectedFilterValues.forEach((filterValue: string) => {
      newFilterVal += filterValue + ',';
    });

    newFilterVal = newFilterVal.substring(0, newFilterVal.length - 1);
    this.currentEditKey.FilterValue = newFilterVal;

    this.dialogFilterValue.rightButtonSelect.unsubscribe();
    this.dialogFilterValue.close();
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


  public GetSelectedPrimaryKeys(): SAPKeyNodeTableLoadDto[] {
    const filterValues: SAPKeyNodeTableLoadDto[] = this.grid.selectedRows;

    return filterValues;
  }

}


@Pipe({
  name: 'startsWith'
})
export class AutocompletePipeStartsWith implements PipeTransform {
  public transform(collection: any[], term = '') {
    return collection.filter((item) => item.toString().toLowerCase().startsWith(term.toString().toLowerCase()));
  }
}
