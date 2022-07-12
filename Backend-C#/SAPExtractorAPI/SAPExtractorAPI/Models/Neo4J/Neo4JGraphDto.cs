using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SAPExtractorAPI.Models.Neo4J
{
    public class Neo4JGraphDto
    {
        public List<Neo4JNodeDto> Nodes { get; set; }

        public List<Neo4JRelationDto> Relations { get; set; }

        public Neo4JGraphDto()
        {
            Nodes = new List<Neo4JNodeDto>();
            Relations = new List<Neo4JRelationDto>();
        }
    }
}