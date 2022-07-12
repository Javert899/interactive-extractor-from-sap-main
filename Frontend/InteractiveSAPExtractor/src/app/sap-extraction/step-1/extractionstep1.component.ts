import { AfterViewInit, Component, ElementRef, Injectable, OnInit, ViewChild } from '@angular/core';
import { IgxDialogComponent, IgxGridComponent, IgxLinearProgressBarComponent } from '@infragistics/igniteui-angular';
import { Logger } from 'src/app/base/logger';
import { NodeObjectTypeDto } from 'src/app/models/Neo4JGraphNode';
import { ExtractorAPIService } from 'src/app/services/extractorAPI.service';
import * as vis from 'vis';

@Component({
  selector: 'app-extractionstep1',
  templateUrl: './extractionstep1.component.html',
  styleUrls: ['./extractionstep1.component.scss'],
})
@Injectable({
  providedIn: 'root'
})
export class ExtractionStep1Component implements OnInit, AfterViewInit {
  @ViewChild('siteConfigNetwork', {
    read: ElementRef,
  })
  public networkContainer!: ElementRef;

  @ViewChild('creationTableNodeGrid', { static: true })
  public grid!: IgxGridComponent;


  @ViewChild('nodeInfoDialog', {
    read: IgxDialogComponent,
  })
  public nodeInfoDialog!: IgxDialogComponent;

  @ViewChild('progressBar', {
    read: IgxLinearProgressBarComponent,
  })
  private progressBar!: IgxLinearProgressBarComponent;

  

  public data!: any[];

  private currentObjectTypeName = '';

  public network!: vis.Network;

  // create an array with nodes
  private nodes = new vis.DataSet([]);

  // create an array with edges
  private edges = new vis.DataSet([]);


  title = 'Welcome to Interactive Extractor for SAP ERP!';
  constructor(private extractorApiService: ExtractorAPIService, private logger: Logger) {}


  ngAfterViewInit(): void {
    this.logger.log(0, 'ExtractionStep1 - ngAfterViewInit Start');


    console.log(this.networkContainer);

    try {
       // this.loadVisTree();
    } catch (error) {
      this.logger.error(5, 'ExtractionStep1 - ngAfterViewInit ERROR');
      this.logger.error(5, error);
    }
  }

  ngOnInit() {
    try {
      this.grid.rendered.subscribe((event) => {
        console.log('Grid Rendered');
        this.grid.selectAllRows();
      });
    } catch (error) {
      this.logger.error(5, 'ExtractionStep1 - ngOnInit ERROR');
      this.logger.error(5, error);
    }
  }

  // Gets the Object Type Network by the selected object name.
  public GetObjectTypeNetwork(objectTypeName: string) {
    this.currentObjectTypeName = objectTypeName;


    this.extractorApiService.GetObjectTypeNetwork(objectTypeName).then((result) => {
      const resultData: any = result;

      console.log(result);
      const objectTypes: NodeObjectTypeDto[] = result;

      this.data = resultData.Nodes;

      const nodeItems = this.GetVisNetworkNodeItems(resultData.Nodes);
      const relationItems = this.GetVisNetworkRelationItems(resultData.Relations);



      this.nodes = new vis.DataSet(nodeItems);
      this.edges = new vis.DataSet(relationItems);
      console.log(nodeItems);
      console.log(relationItems);
      


      this.loadVisTree();

      this.grid.selectAllRows();
      this.grid.selectAllRows();
    });
  }



  private GetVisNetworkNodeItems(neo4JNodeData: any): any {
    const nodeItems: any[] = [];
    neo4JNodeData.forEach((node: any) => {
      const foundItem = nodeItems.filter((x) => x.id === node.Id);

      if (foundItem.length === 0) {
        let color = '#00549f';
        const nodeLabel: string = node.Label;

        if(nodeLabel.indexOf('ObjectType') >= 0) {
          color = '#84c5e0';
        } else if(nodeLabel.indexOf('RECORDTABLE') >= 0) {
          color = '#d6706b';
        } else if(nodeLabel.indexOf('TABLE') >= 0) {
          color = '#f5e08e';
        }

        const nodeItem = {
          id: node.Id,
          label: node.Name,
          color: color,
        };



        nodeItems.push(nodeItem);
      }
    });

    return nodeItems;
  }

  private GetVisNetworkRelationItems(neo4JRelationData: any): any {
    const relationItems: any[] = [];
    neo4JRelationData.forEach((relation: any) => {
      const relationItem = {
        from: relation.Node1Id,
        to: relation.Node2Id,
      };

      relationItems.push(relationItem);
    });

    return relationItems;
  }

  loadVisTree() {

    this.logger.log(0, 'ExtractionStep1 - loadVisTree - Start' );

    var options = {
      interaction: {
        hover: true,
      },
      manipulation: {
        enabled: true,
      },
      nodes: {
        shape: 'dot',
      },
      layout: {
        randomSeed: undefined,
        improvedLayout: false,
      },
    };
    // create a network
    const container = document.getElementById('mynetwork') as HTMLElement;
    // var container = this.networkContainer.nativeElement;

    var data = {
      nodes: this.nodes,
      edges: this.edges,
    };

    this.network = new vis.Network(container, data, options);

    this.network.setOptions(options);


    /*
    this.network.on('hoverNode', (params: any) => {
      console.log('hoverNode Event:', params);
      // this.nodeInfoDialog.open();
    });
*/


    this.network.on('stabilizationProgress', (params: any) => {
      var maxWidth = 496;
      var minWidth = 20;
      var widthFactor = params.iterations / params.total;
      var width = Math.max(minWidth, maxWidth * widthFactor);
      const percentValue = Math.round(widthFactor * 100);
      
      console.log(percentValue + '%');

      this.progressBar.valueInPercent = percentValue;
      
      /*
    document.getElementById("bar").style.width = width + "px";
    document.getElementById("text").innerText =
      Math.round(widthFactor * 100) + "%";
      */
    });
    this.network.once('stabilizationIterationsDone', (params: any) => {
      console.log('Network Loaded');

      this.progressBar.valueInPercent = 100;

      this.grid.selectAllRows();
    });

    this.network.on('click', (params: any) => {
      console.log('ClickNode Event:', params);

    });


    this.logger.log(0, 'ExtractionStep1 - loadVisTree - End' );    	
  }
}
