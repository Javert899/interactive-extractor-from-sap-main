// Represents a Relation from Neo4J
export class Neo4JRelationDto {
    Id!: number;
    RelationshipType!: string;
    Node1Id!: number;
    Node2Id!: number;
  }