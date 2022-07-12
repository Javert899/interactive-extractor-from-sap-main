using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace SAPExtractorAPI.Models.Neo4J
{
    [Serializable, DataContract]
    public class Neo4jNodePropertyDto
    {

        public Neo4jNodePropertyDto()
        {

        }

        /// <summary>
        /// Copy Konstruktor
        /// </summary>
        /// <param name="copy">Das zu kopierende Objekt</param>
        public Neo4jNodePropertyDto(Neo4jNodePropertyDto copy)
        {
            PropertyName = copy.PropertyName;
            Value = copy.Value;
            NodeId = copy.NodeId;
        }



        /// <summary>
        /// Bleibt beim alten Wert, damit die Eigenschaft wiedergefunden werden kann
        /// </summary>
        [DataMember] public string PropertyName { get; set; }

        /// <summary>
        /// Wert der Eigenschaft
        /// </summary>
        [DataMember] public object Value { get; set; }
        /// <summary>
        /// Id des zugehoerigen Knotens
        /// </summary>
        [DataMember] public long NodeId { get; set; }
    }
}
