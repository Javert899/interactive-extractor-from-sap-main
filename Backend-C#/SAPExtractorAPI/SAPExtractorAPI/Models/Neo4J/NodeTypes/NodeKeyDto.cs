using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace SAPExtractorAPI.Models.Neo4J.NodeTypes
{
    [Serializable, DataContract]
    public class NodeKeyDto
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Alias { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public List<string> Labels { get; set; }

        [DataMember]
        public long Neo4JId { get; set; }

        [DataMember]
        public string DATATYPE { get; set; }

        [DataMember]
        public string DOMNAME { get; set; }

        [DataMember]
        public string ROLLNAME { get; set; }

        [DataMember]
        public string FIELDNAME { get; set; }

        [DataMember]
        public int Position { get; set;  }

        [DataMember]
        public List<string> PossibleValues { get; set; }
    }
}