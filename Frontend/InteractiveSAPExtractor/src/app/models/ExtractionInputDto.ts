import { SAPKeyNodeTableLoadDto } from "./PrimaryKeyDto";
import { Neo4JNodeDto, SAPTableNodeDto } from "./RelevantTableGraphDto";

export class ExtractionInputDto {
    Tables!:  Neo4JNodeDto[];    
    PrimaryKeys!: SAPKeyNodeTableLoadDto[]
  }