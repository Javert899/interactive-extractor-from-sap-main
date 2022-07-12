using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SAPExtractorAPI.Models.Neo4J.Relation
{
    /// <summary>
    /// Dient zur Speicherung eines Pfades die Reihenfolge der Verbindungen spiegelt den Weg wider
    /// </summary>
    public class Neo4JRelationPath
    {
        public Neo4JRelationPath()
        {
            RelationDtos = new List<Neo4JRelationDto>();
        }
        public List<Neo4JRelationDto> RelationDtos { get; set; }
    }
}