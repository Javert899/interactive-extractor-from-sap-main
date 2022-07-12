using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SAPExtractorAPI.Models.Neo4J.Node
{
    /// <summary>
    /// Vergleicht zwei <see cref="Neo4JNodeDto"/>s anhand der Id.
    /// </summary>
    public class NodeBaseComparer : IEqualityComparer<Neo4JNodeDto>
    {
        /// <summary>
        /// Zwei <see cref="Neo4JNodeDto"/>s gelten als identisch, wenn die Id dieselbe ist.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool Equals(Neo4JNodeDto x, Neo4JNodeDto y)
        {
            if (x == null || y == null)
                return false;

            return x.Id == y.Id;
        }

        public int GetHashCode(Neo4JNodeDto obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}