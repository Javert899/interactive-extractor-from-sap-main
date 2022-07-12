using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SAPExtractorAPI.Models.Neo4J.Relation
{
    public class RelationBaseComparer : IEqualityComparer<Neo4JRelationDto>
    {
        public bool Equals(Neo4JRelationDto x, Neo4JRelationDto y)
        {
            if (x == null || y == null)
                return false;

            return x.Id == y.Id;

            //return (x.Node1Id == y.Node1Id && x.Node2Id == y.Node2Id) || (x.Node1Id == y.Node2Id && x.Node2Id == y.Node1Id);
        }

        public int GetHashCode(Neo4JRelationDto obj)
        {
            return (int)(obj.Node1Id + obj.Node2Id) | 2;
        }
    }
}
