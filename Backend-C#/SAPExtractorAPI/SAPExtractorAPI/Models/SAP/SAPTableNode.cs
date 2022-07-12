using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SAPExtractorAPI.Models.Neo4J;
using SAPExtractorAPI.Models.Neo4J.NodeTypes;

namespace SAPExtractorAPI.Models.SAP
{
    public class SAPTableNode
    {
        public SAPTableNode()
        {
            this.PrimaryKeyNodes = new List<NodeKeyDto>();
        }

        public NodeTableDto TableNode { get; set; }

        public List<NodeKeyDto> PrimaryKeyNodes { get; set; }

    }
}