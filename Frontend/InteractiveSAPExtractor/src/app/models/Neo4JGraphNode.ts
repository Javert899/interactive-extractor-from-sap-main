
// Represents a Node from Neo4J
export class Neo4JGraphNode {
    id!: number;
    name!: string;
}


export class NodeObjectTypeDto {

    Id!: number;
    Name!: string;
    Alias!: string;
    Description!: string;
    ENTRYCOUNT!: string;
    Labels!: string[];
    Neo4JId!: number;
}


export class NodePropertyDto {
    Name!: string;    
    Value!: string
  }