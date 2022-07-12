using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SAPExtractorAPI.Models.Neo4J
{
    public class Neo4JPropertyCollectionNodeDto
    {
        public class Property
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }

        public List<Property> Properties { get; set; }
        public long Id { get; set; }

    }
}