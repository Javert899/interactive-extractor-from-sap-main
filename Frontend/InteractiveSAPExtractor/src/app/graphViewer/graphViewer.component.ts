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
} from '@infragistics/igniteui-angular';
import * as vis from 'vis';
import { NodeObjectTypeDto } from '../models/Neo4JGraphNode';
import { ExtractorAPIService } from '../services/extractorAPI.service';

@Component({
  selector: 'app-graphviewer',
  templateUrl: './graphViewer.component.html',
  styleUrls: ['./graphViewer.component.scss'],
})
export class GraphViewerComponent implements OnInit, AfterViewInit {
  @ViewChild('siteConfigNetwork', {
    read: ElementRef,
  })
  public networkContainer!: ElementRef;

  @ViewChild('nodeInfoDialog', {
    read: IgxDialogComponent,
  })
  public nodeInfoDialog!: IgxDialogComponent;
  public data!: NodeObjectTypeDto[];

  title = 'Welcome to Ignite UI for Angular!';
  public network!: vis.Network;

  // create an array with nodes
  private nodes = new vis.DataSet([
    {
      id: 1,
      label: 'Node 1',
    },
    {
      id: 2,
      label: 'Node 2',
    },
    {
      id: 3,
      label: 'Node 3',
    },
    {
      id: 4,
      label: 'Node 4',
    },
    {
      id: 5,
      label: 'Node 5',
    },
  ]);

  // create an array with edges
  private edges = new vis.DataSet([
    {
      from: 1,
      to: 3,
    },
    {
      from: 1,
      to: 2,
    },
    {
      from: 2,
      to: 4,
    },
    {
      from: 2,
      to: 5,
    },
    {
      from: 3,
      to: 3,
    },
  ]);

  constructor(private extractorApiService: ExtractorAPIService) {}

  ngAfterViewInit(): void {
    console.log(this.networkContainer);

    this.loadVisTree();
  }

  ngOnInit() {
    this.extractorApiService.GetObjectTypesNetwork().then((result) => {
      const resultData: any = result;

      console.log(result);
      const objectTypes: NodeObjectTypeDto[] = result;

      var visNodesDataSet = new vis.DataSet(resultData.nodes);
      this.data = objectTypes;

      const nodeItems = this.GetVisNetworkNodeItems(resultData.Nodes);
      const relationItems = this.GetVisNetworkRelationItems(resultData.Relations);



      this.nodes = new vis.DataSet(nodeItems);
      this.edges = new vis.DataSet(relationItems);
      console.log(nodeItems);
      console.log(relationItems);

      this.loadVisTree();
    });
  }

  private GetVisNetworkNodeItems(neo4JNodeData: any): any {
    const nodeItems: any[] = [];
    neo4JNodeData.forEach((node: any) => {
      const foundItem = nodeItems.filter((x) => x.id === node.Id);

      if (foundItem.length === 0) {
        let color = '#00549f';
        switch (node.Label) {
          case 'SAP:ObjectType':
            color = '#00BFFF';
            break;

          case 'SAP:TABLE:CLUSTERING':
            color = '#FFFACD'
            break;
          default:
            break;
        }

        const nodeItem = {
          id: node.Id,
          label: node.Name,
          color: color
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
          to: relation.Node2Id
        };

        relationItems.push(relationItem);
    });

    return relationItems;
  }


  loadVisTree() {
    var options = {
      interaction: {
        hover: true,
      },
      manipulation: {
        enabled: true,
      },
      nodes: {
        shape: "dot",
      },
      layout: {
        randomSeed: undefined,
        improvedLayout:false,
      }
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

    this.network.on('hoverNode', (params: any) => {
      console.log('hoverNode Event:', params);
      this.nodeInfoDialog.open();
    });
    this.network.on('blurNode', function (params: any) {
      console.log('blurNode event:', params);
    });

    
  this.network.on("stabilizationProgress", function (params) {
    var maxWidth = 496;
    var minWidth = 20;
    var widthFactor = params.iterations / params.total;
    var width = Math.max(minWidth, maxWidth * widthFactor);
    console.log(Math.round(widthFactor * 100) + "%");

    /*
    document.getElementById("bar").style.width = width + "px";
    document.getElementById("text").innerText =
      Math.round(widthFactor * 100) + "%";
      */
  });
  this.network.once("stabilizationIterationsDone", function () {
    /*
    document.getElementById("text").innerText = "100%";
    document.getElementById("bar").style.width = "496px";
    document.getElementById("loadingBar").style.opacity = 0;
    // really clean the dom element
    setTimeout(function () {
      document.getElementById("loadingBar").style.display = "none";
    }, 500);
    */
  });
  }
}
