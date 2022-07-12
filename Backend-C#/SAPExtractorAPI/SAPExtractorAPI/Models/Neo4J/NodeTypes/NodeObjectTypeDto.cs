using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace SAPExtractorAPI.Models.Neo4J.NodeTypes
{
    public class NodeObjectTypeDto
    {

        public long Id { get; set; }

        public string Name { get; set; }

        public string Alias { get; set; }

        public string Description { get; set; }

        public int ENTRYCOUNT { get; set; }

        public List<string> Labels { get; set; }

        public long Neo4JId { get; set; }
    }

    [Serializable, DataContract]
    public class NodeTableDto
    {
        [DataMember]
        public long Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Alias { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string ENTRYCOUNT { get; set; }

        [DataMember]
        public List<string> Labels { get; set; }

        [DataMember]
        public long Neo4JId { get; set; }
    }

    public class NodeObjectTypeNetworkDto
    {
        public IEnumerable<NodeObjectTypeDto> ObjectTypeDtos { get; set; }
        public IEnumerable<NodeTableDto> TableDtos { get; set; }
    }
}