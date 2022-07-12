using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SAPExtractorAPI.Models.Neo4J.NodeTypes
{
    public class RelationCreatedInDto
    {
        public string ENTRYCOUNT { get; set; }
    }

    public class RelationDto
    {
        public string StartId { get; set; }
        public string EndId { get; set; }
        public string ENTRYCOUNT { get; set; }
    }
}