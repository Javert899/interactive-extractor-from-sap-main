using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace SAPExtractorAPI.Models.Neo4J.Relation
{
    /// <summary>
    /// Eigenschaft einer Verbindung
    /// </summary>
    [Serializable, DataContract]
    public class Neo4JRelationPropertyDto
    {
        public Neo4JRelationPropertyDto() { }
        /// <summary>
        /// Copy Konstruktor
        /// </summary>
        /// <param name="copy">Das zu kopierende Objekt</param>
        public Neo4JRelationPropertyDto(Neo4JRelationPropertyDto copy)
        {
            PropertyName = copy.PropertyName;
            Value = copy.Value;
        }

        /// <summary>
        /// Bleibt beim alten Wert, damit die Eigenschaft wiedergefundem werden kann
        /// </summary>
        [DataMember]
        public string PropertyName { get; set; }

        /// <summary>
        /// Wert der Eigenschaft
        /// </summary>
        [DataMember]
        public object Value { get; set; }
        /// <summary>
        /// Id der zugehoerigen Kante
        /// </summary>
        [DataMember]
        public long RelationId { get; set; }
    }
}
