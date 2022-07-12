using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Neo4jClient;
using Newtonsoft.Json.Linq;

namespace SAPExtractorAPI.Models.Neo4J
{
    public class Neo4JNodeLabelsDto
    {
        public Node<string> Component { get; set; }
        public IEnumerable<string> Labels { get; set; }
    }

    public class Neo4JNodeLabelsDto2
    {
        public JObject Component { get; set; }
        public IEnumerable<string> Labels { get; set; }
    }
}