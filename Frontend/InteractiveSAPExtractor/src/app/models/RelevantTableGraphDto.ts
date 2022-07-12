import { Neo4JRelationDto } from "./Neo4JRelationDto";
import { SAPKeyNodeDto } from "./PrimaryKeyDto";

export class RelevantTableGraphDto {
    Tables!: SAPTableNodeDto[];
    ObjectTypes!: Neo4JNodeDto[];
    Relations!: Neo4JRelationDto[];
}

export class Neo4JNodeDto {
  Id!: number;
  Name!: string;
  Alias!: string;
  Description!: string;
  ENTRYCOUNT!: number;
  Labels!: string[];
  Neo4JId!: number;
}

export class SAPTableNodeDto {
    TableNode!: Neo4JNodeDto;
    PrimaryKeyNodes!: SAPKeyNodeDto[];
}



