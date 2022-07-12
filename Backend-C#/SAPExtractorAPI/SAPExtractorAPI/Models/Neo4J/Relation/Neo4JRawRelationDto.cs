using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Neo4jClient;

namespace SAPExtractorAPI.Models.Neo4J.Relation
{
    public class Neo4JRawRelationDto
    {
        public long Node1Id { get; set; }
        public long Node2Id { get; set; }
        public RelationshipInstance<Dictionary<string, string>> Relation { get; set; }
        public string RelationshipType { get; set; }
    }
}