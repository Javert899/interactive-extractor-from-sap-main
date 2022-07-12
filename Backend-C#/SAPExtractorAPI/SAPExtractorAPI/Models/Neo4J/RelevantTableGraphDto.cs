using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SAPExtractorAPI.Models.Neo4J.NodeTypes;
using SAPExtractorAPI.Models.SAP;

namespace SAPExtractorAPI.Models.Neo4J
{
    public class RelevantTableGraphDto
    {
        public List<SAPTableNode> Tables { get; set; }

        public List<NodeObjectTypeDto> ObjectTypes { get; set; }

        public List<Neo4JRelationDto> Relations { get; set; }

        public RelevantTableGraphDto()
        {
            Tables = new List<SAPTableNode>();
            ObjectTypes = new List<NodeObjectTypeDto>();
            Relations = new List<Neo4JRelationDto>();
        }
    }
}