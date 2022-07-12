import { Injectable } from "@angular/core";
import { AppConfig } from "../base/app-config";
import { Logger } from "../base/logger";
import { SAPKeyNodeDto } from "../models/PrimaryKeyDto";
import { Neo4JNodeDto } from "../models/RelevantTableGraphDto";

@Injectable({
    providedIn: 'root'
  })
  export class VisNetworkHelper {

  
    apiUrl = this.localConfig.getConfig('apiUrl');
  
    constructor(private localConfig: AppConfig, private logger: Logger) {
  
    }


    public GetVisNetworkNodeItems(neo4JNodeData: Neo4JNodeDto[]): any {
        console.log(neo4JNodeData);

        const nodeItems: any[] = [];
        neo4JNodeData.forEach((node: Neo4JNodeDto) => {
          const foundItem = nodeItems.filter((x) => x.id === node.Id);
          let entryCount = -1;

          if (foundItem.length === 0) {
            let color = '#00549f';
            // console.warn(node);
            
            if(node.Labels.find(x => x === 'ObjectType') != undefined) {
              color = '#84c5e0';
            } else if(node.Labels.find(x => x === 'RECORDTABLE') != undefined) {
              color = '#d6706b';
            } else if(node.Labels.find(x => x === 'TABLE') != undefined) {
              color = '#f5e08e';

              entryCount = node.ENTRYCOUNT;
            }
            
    
            /*
            switch (node.Label) {
              case 'SAP:ObjectType':
                color = '#00BFFF';
                break;
    
              case 'SAP:TABLE:CLUSTERING':
                color = '#FFFACD';
                break;
              default:
                break;
            }
            */
            
            const nodeItem = {
              id: node.Id,
              label: node.Name,
              color: color,
              value: -1
            };

            if(node.Labels.find(x => x === 'TABLE') != undefined) {
              nodeItem.value = entryCount;
            }
    
            nodeItems.push(nodeItem);
          }
        });
    
        return nodeItems;
      }


      public AppendPrimaryKeysToNodes(nodeItems: any, tempPrimaryKeys: SAPKeyNodeDto[]): any {
        

        tempPrimaryKeys.forEach((primKey: SAPKeyNodeDto) => {
            let color = '#a0c998';

            const nodeItem = {
                id: primKey.Id,
                label: primKey.Name,
                color: color,
              };
      
              nodeItems.push(nodeItem);
        });

        return nodeItems;
      }
  }
  