using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SAPExtractorAPI.Models.Neo4J.Relation;

namespace SAPExtractorAPI.Models.Neo4J
{
    /// <summary>
    /// DTO welches eine Verbindung zwischen Zwei Neo4j Knoten widerspiegelt
    /// </summary>
    public class Neo4JRelationDto
    {
        public Neo4JRelationDto()
        {
            Properties = new List<Neo4JRelationPropertyDto>();
            Id = -1;
        }
        /// <summary>
        /// Copy Konstruktor
        /// </summary>
        /// <param name="copy">Das zu kopierende Objekt</param>
        public Neo4JRelationDto(Neo4JRelationDto copy)
            : this()
        {
            Node1Id = copy.Node1Id;
            Node2Id = copy.Node2Id;
            RelationshipType = copy.RelationshipType;
            foreach (Neo4JRelationPropertyDto property in copy.Properties)
            {
                Properties.Add(new Neo4JRelationPropertyDto(property));
            }
        }

        /// <summary>
        /// Interne Neo4JRelationId
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// Id der Quelle
        /// </summary>
        public long Node1Id { get; set; }
        /// <summary>
        /// Id des Ziels
        /// </summary>
        public long Node2Id { get; set; }
        /// <summary>
        /// Relationship Typ 
        /// </summary>
        public string RelationshipType { get; set; }
        /// <summary>
        /// Eigenschaften
        /// </summary>
        public List<Neo4JRelationPropertyDto> Properties { get; set; }
    }

}