using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Neo4jClient;

namespace SAPExtractorAPI.Models.Neo4J.Relation
{
    public class Neo4jNodeRelationEntityDtoList
    {
        public Node<string> StartComponent { get; set; }

        public IEnumerable<string> StartLabels { get; set; }

        public List<RelationshipInstance<Dictionary<string, string>>> Relations { get; set; }

        public string RelationshipType { get; set; }

        public Node<string> EndComponent { get; set; }

        public IEnumerable<string> EndLabels { get; set; }
    }

    public class Neo4jNodeRelationEntityDtoListAPOC
    {
        public List<Node<string>> Nodes { get; set; }

        public List<RelationshipInstance<Dictionary<string, string>>> Relations { get; set; }

        public IEnumerable<Neo4jNodeLabelDtoAPOC> Labels { get; set; }
    }

    public class Neo4jNodeLabelDtoAPOC
    {
        public int Id { get; set; }
        public List<String> label { get; set; }
    }

    public class Neo4jNodeRelationEntityDtoListDirected
    {
        public IEnumerable<Node<string>> Nodes { get; set; }

        public IEnumerable<RelationshipInstance<Dictionary<string, string>>> Relationships { get; set; }
    }
}