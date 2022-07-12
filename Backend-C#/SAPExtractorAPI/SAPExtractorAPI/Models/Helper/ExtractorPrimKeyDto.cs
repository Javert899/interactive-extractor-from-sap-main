using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SAPExtractorAPI.Models.Neo4J;
using SAPExtractorAPI.Models.Neo4J.NodeTypes;
using SAPExtractorAPI.Models.SAP;

namespace SAPExtractorAPI.Models.Helper
{
    public class ExtractorPrimKeyDto
    {
        public NodeKeyDto fieldDto { get; set; }
        public List<SAPTableNode> tables { get; set; }
    }
}