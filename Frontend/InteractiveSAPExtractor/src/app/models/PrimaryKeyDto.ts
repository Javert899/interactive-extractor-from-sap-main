import { Neo4JNodeDto, SAPTableNodeDto } from "./RelevantTableGraphDto";

export class PrimaryKeyDto {
    Tables!: Neo4JNodeDto[];
    Name!: string;    
}


export class PrimaryKeyPreprocessDto {
    Name!: string; 
    Id!: number;
    Alias!: string;
    Description!: string;
    Labels!: string[];
    Neo4JId!: number;
    Datatype!: string;
    Domname!: string;
    Rollname!: string;
    Fieldname!: string;
    Position!: number;
    // Value the User Can Enter
    Value!: string;
}

export class SAPKeyNodeDto {
    Id!: number;
    Name!: string;
    Alias!: string;
    Description!: string;
    Labels!: string[];
    Neo4JId!: number;
    DATATYPE!: string;
    DOMNAME!: string;
    ROLLNAME!: string;
    FIELDNAME!: string;
    Position!: number;
    PossibleValues!: string[];
  }

  export class SAPKeyNodeTableLoadDto {
    KeyNode!: SAPKeyNodeDto;
    Loading!: boolean;
    FilterValue!: string;
  }

  
export class ExtractorPrimKeyDto {
    fieldDto!: SAPKeyNodeDto;
    tables!: SAPTableNodeDto[];
  }