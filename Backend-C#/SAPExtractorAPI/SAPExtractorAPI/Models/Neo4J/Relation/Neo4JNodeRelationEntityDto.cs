using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Neo4jClient;

namespace SAPExtractorAPI.Models.Neo4J.Relation
{
    public class Neo4JNodeRelationEntityDto
    {
        public Node<string> StartComponent { get; set; }

        public IEnumerable<string> StartLabels { get; set; }

        public RelationshipInstance<Dictionary<string, string>> Relation { get; set; }

        public string RelationshipType { get; set; }

        public Node<string> EndComponent { get; set; }

        public IEnumerable<string> EndLabels { get; set; }
    }
}