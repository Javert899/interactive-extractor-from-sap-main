import {
  AfterViewInit,
  Component,
  ElementRef,
  OnInit,
  ViewChild
} from '@angular/core';
import {
  IgxGridComponent,
  IgxDialogComponent,
  IgxLinearProgressBarComponent
} from '@infragistics/igniteui-angular';
import {
  Logger
} from 'src/app/base/logger';
import {
  VisNetworkHelper
} from 'src/app/helper/VisNetworkHelper';
import {
  NodeObjectTypeDto,
  NodePropertyDto
} from 'src/app/models/Neo4JGraphNode';
import {
  SAPKeyNodeDto
} from 'src/app/models/PrimaryKeyDto';
import {
  Neo4JNodeDto,
  SAPTableNodeDto
} from 'src/app/models/RelevantTableGraphDto';
import {
  ExtractorAPIService
} from 'src/app/services/extractorAPI.service';
import * as vis from 'vis';

@Component({
  selector: 'app-extractionstep2',
  templateUrl: './extractionstep2.component.html',
  styleUrls: ['./extractionstep2.component.scss']
})
export class ExtractionStep2Component implements OnInit, AfterViewInit {
  @ViewChild('siteConfigNetwork', {
    read: ElementRef,
  })
  public networkContainer!: ElementRef;

  @ViewChild('creationTableNodeGrid', {
    static: true
  })
  public grid!: IgxGridComponent;


  @ViewChild('nodeInfoDialog', {
    read: IgxDialogComponent,
  })
  public nodeInfoDialog!: IgxDialogComponent;

  @ViewChild('progressBar', {
    read: IgxLinearProgressBarComponent,
  })
  private progressBar!: IgxLinearProgressBarComponent;



  public data!: Neo4JNodeDto[];
  public Tables!: SAPTableNodeDto[];
  public ObjectTypes!: Neo4JNodeDto[];
  public PrimaryKeys!: SAPKeyNodeDto[];

  private currentObjectTypeName = '';

  public currentClickedNode: any;
  public currentClickedNodeProperties: NodePropertyDto[] = [];

  public network!: vis.Network;

  // create an array with nodes
  private nodes = new vis.DataSet([]);

  // create an array with edges
  private edges = new vis.DataSet([]);


  title = 'Welcome to Interactive Extractor for SAP ERP!';
  constructor(private extractorApiService: ExtractorAPIService, private logger: Logger, private visNetworkHelper: VisNetworkHelper) {}


  ngAfterViewInit(): void {
    this.logger.log(0, 'ExtractionStep2 - ngAfterViewInit Start');
    console.log(this.networkContainer);

    // this.loadVisTree();
  }

  ngOnInit() {

    this.grid.rendered.subscribe((event) => {
      console.log('Grid Rendered');
      this.grid.selectAllRows();
    });
  }

  public GetObjectTypeNetwork(objectTypeName: string) {
    this.logger.log(0, 'GetObjectTypeNetwork');


    try {
      this.currentObjectTypeName = objectTypeName;

      // Get ObjectTypeNetwork from Backend.
      this.extractorApiService.GetRelevantTablesForObjectType(objectTypeName, 5000).then((result) => {
        this.logger.log(0, 'Received ObjectTypeNetwork');
        this.logger.log(0, result);
        const tempNetworkNodes: Neo4JNodeDto[] = [];
        const tempTableNodes: Neo4JNodeDto[] = [];
  
        const tempPrimaryKeys: SAPKeyNodeDto[] = [];
  
        // Set Tables for Network Display
        result.Tables.forEach(table => {
          tempNetworkNodes.push(table.TableNode);
          tempTableNodes.push(table.TableNode);
  
          table.PrimaryKeyNodes.forEach(primaryKey => {
            const found = tempPrimaryKeys.filter(x => x.Name === primaryKey.Name);
  
            if (found.length === 0) {
              tempPrimaryKeys.push(primaryKey);
            }
          });
        });
  
        this.Tables = result.Tables;
        this.ObjectTypes = result.ObjectTypes;
        this.PrimaryKeys = tempPrimaryKeys;
        this.data = tempTableNodes;
  
        result.ObjectTypes.forEach(objectType => {
          tempNetworkNodes.push(objectType);
        });
  
        let nodeItems = this.visNetworkHelper.GetVisNetworkNodeItems(tempNetworkNodes);
  
        nodeItems = this.visNetworkHelper.AppendPrimaryKeysToNodes(nodeItems, tempPrimaryKeys);
  
        let relationItems = this.GetVisNetworkRelationItems(result.Relations);
        relationItems = this.GetVisNetworkRelationItems(result.Relations);
  
        this.nodes = new vis.DataSet(nodeItems);
        this.edges = new vis.DataSet(relationItems);
  
        this.loadVisTree();
      });
  
    } catch (error) {
      this.logger.error(5, 'ExtractionStep2 - GetObjectTypeNetwork - ERROR');
      this.logger.error(5, error);
    }
  }

  public GetSelectedTables(): SAPTableNodeDto[] {

    const selectedTables: SAPTableNodeDto[] = [];
    const selectedIds = this.grid.selectedRows;

    console.log(this.data);

    selectedIds.forEach(id => {
      const foundItem = this.Tables.filter(x => x.TableNode.Id === id);
      if (foundItem.length > 0) {
        console.log(foundItem[0]);
        selectedTables.push(foundItem[0]);
      }
    })

    return selectedTables;
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
    var options = {
      interaction: {
        hover: true,
      },
      manipulation: {
        enabled: true,
      },
      nodes: {
        shape: 'dot',
        scaling: {
          label: {
            min: 14,
            max: 22,
          },
          customScalingFunction: function (min: any, max: any, total: number, value: number) {
            if (max === min) {
              return 0.5;
            }
            else {
              var scale = 1 / (max - min);
              return Math.max(0,(value - min)*scale);
            }
          },
          min: 30,
          max: 45,
        },
      },
      layout: {
        randomSeed: undefined,
        improvedLayout: true,
        hierarchical: false
      },
      physics: {
        // Even though it's disabled the options still apply to network.stabilize().
        enabled: true,
        solver: "repulsion",
        repulsion: {
          nodeDistance: 200 // Put more distance between the nodes.
        }
      }
    };
    // create a network
    const container = document.getElementById('mynetwork') as HTMLElement;

    var data = {
      nodes: this.nodes,
      edges: this.edges,
    };

    this.network = new vis.Network(container, data, options);

    this.network.setOptions(options);



    this.network.on('stabilizationProgress', (params: any) => {
      var maxWidth = 496;
      var minWidth = 20;
      var widthFactor = params.iterations / params.total;
      var width = Math.max(minWidth, maxWidth * widthFactor);
      const percentValue = Math.round(widthFactor * 100);

      console.log(percentValue + '%');

      this.progressBar.valueInPercent = percentValue;
    });
    this.network.once('stabilizationIterationsDone', (params: any) => {
      console.log('Network Loaded');

      this.progressBar.valueInPercent = 100;

      this.grid.selectAllRows();


      var options = {
        interaction: {
          hover: true,
        },
        manipulation: {
          enabled: true,
        },
        nodes: {
          shape: 'dot',
          scaling: {
            label: {
              min: 14,
              max: 22,
            },
            customScalingFunction: function (min: any, max: any, total: number, value: number) {
              if (max === min) {
                return 0.5;
              }
              else {
                var scale = 1 / (max - min);
                return Math.max(0,(value - min)*scale);
              }
            },
            min: 30,
            max: 45,
          },
        },
        layout: {
          randomSeed: undefined,
          improvedLayout: true,
          hierarchical: false
        },
        physics: {
          // Even though it's disabled the options still apply to network.stabilize().
          enabled: false,
          solver: "repulsion",
          repulsion: {
            nodeDistance: 200 // Put more distance between the nodes.
          }
        }
      };


      this.network.setOptions(options);
    });




    this.network.on('click', (params: any) => {
      this.CheckNodeOnClick(params);
    });


    this.network.on('doubleClick', (params: any) => {
      this.ProcessNodeOnDoubleClick(params);
    });
  }

  doubleClickTime = new Date();
  threshold = 200;


  // Processes a node on click.
  private CheckNodeOnClick(params: any) {

    const t0 = new Date();
    if (t0.getTime() - this.doubleClickTime.getTime() > this.threshold) {
      setTimeout( () => {
        if (t0.getTime() - this.doubleClickTime.getTime() > this.threshold) {
          this.ProcessNodeOnClick(params);
        }
      },this.threshold);
    }
  }

  private ProcessNodeOnClick(params: any) {

    console.log('ClickNode Event:', params);

    // Check if params cotain nodes
    if (params.nodes.length > 0) {
      this.nodeInfoDialog.open();
      this.nodeInfoDialog.closeOnOutsideSelect = true;

      this.currentClickedNodeProperties = [];

      const nodeId: number = params.nodes[0];

      const correspondingTableNode = this.Tables.find(x => x.TableNode.Id === nodeId);
      let nodeLabel = '';


      if(correspondingTableNode !== undefined) {
        console.log('IsTable');
        nodeLabel = 'TABLE';
      } else {
        const correspondingObjectTypeNode = this.ObjectTypes.find(x => x.Id === nodeId);
        if(correspondingObjectTypeNode !== undefined) {
          console.log('IsObjectType');
          nodeLabel = 'ObjectType';
        } else {
          const correspondingKeyNode = this.PrimaryKeys.find(x => x.Id === nodeId);
          console.log('IsKey');
          nodeLabel = 'KEY';
        }
      }

      console.log(nodeLabel.search('KEY') !== undefined);
      // Check of what type the clicked node is.
      if (nodeLabel ==='KEY') {
        const foundPrimaryKeys = this.PrimaryKeys.filter(x => x.Id === params.nodes[0]);

        if (foundPrimaryKeys.length > 0) {
          const foundPrimaryKey = foundPrimaryKeys[0];

          this.currentClickedNodeProperties.push({
            Name: 'Name',
            Value: foundPrimaryKey.Alias
          });

          this.currentClickedNodeProperties.push({
            Name: 'Description',
            Value: foundPrimaryKey.Description
          });

          this.currentClickedNodeProperties.push({
            Name: 'Domain',
            Value: foundPrimaryKey.DOMNAME
          });

          this.currentClickedNodeProperties.push({
            Name: 'Role',
            Value: foundPrimaryKey.ROLLNAME
          });
        }

      } else if (nodeLabel ==='TABLE') {
        const foundTables = this.Tables.filter(x => x.TableNode.Neo4JId === params.nodes[0]);
        if (foundTables.length > 0) {
          const foundTable = foundTables[0];
          this.currentClickedNodeProperties.push({
            Name: 'Name',
            Value: foundTable.TableNode.Alias
          });
          this.currentClickedNodeProperties.push({
            Name: 'Description',
            Value: foundTable.TableNode.Description
          });
          this.currentClickedNodeProperties.push({
            Name: 'Entrycount',
            Value: '' + foundTable.TableNode.ENTRYCOUNT
          });
          let label = '';
          foundTable.TableNode.Labels.forEach(currLabel => {
            label += '' + currLabel + ':';
          });
  
          this.currentClickedNodeProperties.push({
            Name: 'Labels',
            Value: label
          });

          console.log(this.currentClickedNodeProperties);
        }
      } else if (nodeLabel ==='ObjectType') {
        const foundObjectTypes = this.ObjectTypes.filter(x => x.Id === params.nodes[0]);
        if (foundObjectTypes.length > 0) {
          const foundObjectType = foundObjectTypes[0];

          this.currentClickedNodeProperties.push({
            Name: 'Name',
            Value: foundObjectType.Alias
          });
          this.currentClickedNodeProperties.push({
            Name: 'Description',
            Value: foundObjectType.Description
          });
          this.currentClickedNodeProperties.push({
            Name: 'Entrycount',
            Value: ''+ foundObjectType.ENTRYCOUNT
          });
          let label = '';
          foundObjectType.Labels.forEach(currLabel => {
            label += '' + currLabel + ':';
          });
  
          this.currentClickedNodeProperties.push({
            Name: 'Labels',
            Value: label
          });
        }
      }
    }

  }


  // Processes a node on click.
  private ProcessNodeOnDoubleClick(params: any) {
    this.doubleClickTime = new Date();


    console.log('Double Clicked Node');
    console.log(params);

    if (params.nodes.length > 0) {
      const nodeId: number = params.nodes[0];

      const correspondingTableNode = this.Tables.find(x => x.TableNode.Id === nodeId);
      let nodeLabel = '';


      if(correspondingTableNode !== undefined) {
        console.log('IsTable');
        nodeLabel = 'TABLE';
      } else {
        const correspondingObjectTypeNode = this.ObjectTypes.find(x => x.Id === nodeId);
        if(correspondingObjectTypeNode !== undefined) {
          console.log('IsObjectType');
          nodeLabel = 'ObjectType';
        } else {
          const correspondingKeyNode = this.PrimaryKeys.find(x => x.Id === nodeId);
          console.log('IsKey');
          nodeLabel = 'KEY';
        }
      }

      console.log(nodeLabel.search('KEY') !== undefined);
      // Check of what type the clicked node is.
      if (nodeLabel ==='KEY') {
      
      } else if (nodeLabel ==='TABLE') {
        const foundTables = this.Tables.filter(x => x.TableNode.Neo4JId === params.nodes[0]);
        if (foundTables.length > 0) {
          const foundTable = foundTables[0];
          
          // Get the Checked Tables and their keys

        }
      } else if (nodeLabel ==='ObjectType') {
        const foundObjectTypes = this.ObjectTypes.filter(x => x.Id === params.nodes[0]);
        if (foundObjectTypes.length > 0) {
          const foundObjectType = foundObjectTypes[0];

          // Search for Relevant Tables with the new Object Type
          this.extractorApiService.GetRelevantTablesForObjectType(foundObjectType.Name, 5000).then((result) => {
            this.logger.log(0, 'Received ObjectTypeNetwork');
            this.logger.log(0, result);
            const tempNetworkNodes: Neo4JNodeDto[] = [];
            const tempTableNodes: Neo4JNodeDto[] = [];
      
            const tempPrimaryKeys: SAPKeyNodeDto[] = [];
      
            // Set Tables for Network Display
            result.Tables.forEach(table => {
              tempNetworkNodes.push(table.TableNode);
              tempTableNodes.push(table.TableNode);
      
              table.PrimaryKeyNodes.forEach(primaryKey => {
                const found = tempPrimaryKeys.filter(x => x.Name === primaryKey.Name);
      
                if (found.length === 0) {
                  tempPrimaryKeys.push(primaryKey);
                }
              });
            });
      
            this.Tables = result.Tables;
            this.ObjectTypes = result.ObjectTypes;
            this.PrimaryKeys = tempPrimaryKeys;
            this.data = tempTableNodes;
      
            result.ObjectTypes.forEach(objectType => {
              tempNetworkNodes.push(objectType);
            });
      
            let nodeItems = this.visNetworkHelper.GetVisNetworkNodeItems(tempNetworkNodes);
      
            nodeItems = this.visNetworkHelper.AppendPrimaryKeysToNodes(nodeItems, tempPrimaryKeys);
      
            let relationItems = this.GetVisNetworkRelationItems(result.Relations);
            relationItems = this.GetVisNetworkRelationItems(result.Relations);
      
            this.nodes = new vis.DataSet(nodeItems);
            this.edges = new vis.DataSet(relationItems);
      
            this.loadVisTree();
          });
          
        }
      }
    }

  }

  public CloseDialog() {
    this.nodeInfoDialog.close();
  }
}
