using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using SAPExtractorAPI.Models.Neo4J.NodeTypes;

namespace SAPExtractorAPI.Models.Neo4J
{
    [Serializable, DataContract]
    public class ExtractionInputDto
    {
        [DataMember]
        public List<NodeTableDto> Tables { get; set; }


        [DataMember]
        public List<SAPKeyNodeTableLoadDto> PrimaryKey { get; set; }

    }

    [Serializable, DataContract]
    public class SAPKeyNodeTableLoadDto
    {
        [DataMember]
        public NodeKeyDto KeyNode { get; set; }

        [DataMember]
        public string FilterValue { get; set; }
    }



    
}