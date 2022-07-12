import {
  HttpClient,
  HttpHeaders
} from "@angular/common/http";
import {
  Injectable
} from "@angular/core";
import {
  AppConfig
} from "../base/app-config";
import {
  Logger
} from "../base/logger";
import { ExtractionInputDto } from "../models/ExtractionInputDto";
import {
  Neo4JGraphNode,
  NodeObjectTypeDto
} from "../models/Neo4JGraphNode";
import {
  ExtractorPrimKeyDto,
  SAPKeyNodeDto,
  SAPKeyNodeTableLoadDto
} from "../models/PrimaryKeyDto";
import {
  Neo4JNodeDto,
  RelevantTableGraphDto,
  SAPTableNodeDto
} from "../models/RelevantTableGraphDto";

@Injectable({
  providedIn: 'root'
})
export class ExtractorAPIService {

  apiUrl = this.localConfig.getConfig('apiUrl');

  constructor(private http: HttpClient, private localConfig: AppConfig, private logger: Logger) {

  }

  GetObjectTypeNetwork(objectTypeName: string): Promise < any[] > {
    this.logger.log(0, 'GetObjectTypeNetwork');
    const requestUrl = this.apiUrl + 'SAPExtractor/GetObjectTypesNetwork?objectTypeName=' + objectTypeName;
    return new Promise < any[] > ((resolve, reject) => {
      this.http.get(requestUrl).subscribe((data: any) => {
        this.logger.warn(0, 'Received Object Types for ' + objectTypeName);
        this.logger.log(0, data);

        const nodes: NodeObjectTypeDto[] = data;


        resolve(data);
      }, error => {
        console.error(error);
        reject(error);
      });
    });

  }

  GetRelevantTablesForObjectType(objectTypeName: string, entrycount: number): Promise < RelevantTableGraphDto > {
    this.logger.log(0, 'GetRelevantTablesForObjectType');

    const requestUrl = this.apiUrl + 'SAPExtractor/GetRelevantTablesForObjectType?objectType=' + objectTypeName + "&entrycount=" + entrycount;
    return new Promise < RelevantTableGraphDto > ((resolve, reject) => {
      this.http.get(requestUrl).subscribe((data: any) => {
        this.logger.warn(0, 'Received Relevant Tables for ' + objectTypeName);
        this.logger.log(0, data);

        const nodes: NodeObjectTypeDto[] = data;


        resolve(data);
      }, error => {
        console.error(error);
        reject(error);
      });
    });

  }


  GetPrimaryKeyValues(primaryKey: SAPKeyNodeDto, tables: SAPTableNodeDto[]): Promise < string[] > {
    this.logger.log(0, 'GetPrimaryKey Values');
    const requestUrl = this.apiUrl + 'SAPExtractor/GetFieldValuesForField';
    return new Promise < string[] > ((resolve, reject) => {
      const jsonPrimKey = JSON.stringify(primaryKey)
      
      const extractorApiDto:ExtractorPrimKeyDto = {
        fieldDto: primaryKey,
        tables: tables
      }
        


      const requestBody = JSON.stringify(extractorApiDto);
      const options = { headers: new HttpHeaders().set('Content-Type', 'application/json') };

      this.http.post(requestUrl, requestBody, options).subscribe((data: any) => {

        resolve(data);
      }, error => {
        
        console.warn('Error for Key: ' + primaryKey.Name);
        console.error(error);
        reject(error);
      });


    });

  }

  GetObjectTypesNetwork(): Promise < any[] > {
    this.logger.log(0, 'GetObjectTypesNetwork');
    const requestUrl = this.apiUrl + 'SAPExtractor/GetObjectTypesNetwork';
    return new Promise < any[] > ((resolve, reject) => {
      this.http.get(requestUrl).subscribe((data: any) => {
        this.logger.warn(0, 'Received Object Types');
        this.logger.log(0, data);

        const nodes: NodeObjectTypeDto[] = data;


        resolve(data);
      }, error => {
        console.error(error);
        reject(error);
      });
    });
  }



  GetObjectTypes(): Promise < NodeObjectTypeDto[] > {
    this.logger.log(0, 'GetObjectTypes - Start');

    const requestUrl = this.apiUrl + 'SAPExtractor/GetObjectTypes';

    this.logger.log(0, 'Request: ' + requestUrl);
    return new Promise < NodeObjectTypeDto[] > ((resolve, reject) => {
      this.http.get(requestUrl).subscribe((data: any) => {
        this.logger.warn(0, 'Received Object Types');
        this.logger.log(0, data);

        const nodes: NodeObjectTypeDto[] = data;


        resolve(nodes);
      }, error => {
        console.error(error);
        reject(error);
      });
    });
  }

  // Performs the extraction and sends the selected tables and keys to the backend.
  PerformExtraction(sapTableNodes: SAPTableNodeDto[], selectedPrimaryKeys: SAPKeyNodeTableLoadDto[]): Promise<boolean> {
    this.logger.log(0, 'PerformExtraction - Start');
    this.logger.log(0, 'Extraction Nodes: ' + sapTableNodes.length);

    const tableNodeDtos: Neo4JNodeDto[] = [];


    sapTableNodes.forEach(sapTableNode => {
      const tableNode = sapTableNode.TableNode;

      tableNodeDtos.push(tableNode);
    });

    const requestUrl = this.apiUrl + 'SAPExtractor/PerformExtraction';

    const extractionInputDto: ExtractionInputDto = {
      Tables: tableNodeDtos,
      PrimaryKeys: selectedPrimaryKeys
    }

    const body = JSON.stringify(extractionInputDto)
    const header = new HttpHeaders().set('Content-type', 'application/json');

    this.logger.log(0, 'Request: ' + requestUrl);

    return new Promise < boolean> ((resolve, reject) => {
       
      this.http.post(requestUrl, body, {headers: header}).subscribe((data: any) => {
        this.logger.warn(0, 'Extraction completed');
        this.logger.log(0, data);

        resolve(true);
      }, error => {
        console.error(error);
        reject(error);
      });
    });
  }

}
