using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Neo4jClient;
using Newtonsoft.Json;
using SAPExtractorAPI.Models.Neo4J;

namespace SAPExtractorAPI.Lib.Mapper
{
    public class BaseNeo4JNodeMapper
    {
        /// <summary>
        /// Gibt bei uebergebener Node ein Dto zurück
        /// in dem alle Eigenschaften gespeichert sind
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected static Neo4JPropertyCollectionNodeDto ToBusinessObject(Node<string> node)
        {

            //Json string deserialisieren
            Dictionary<string, string> properties =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(node.Data);

            //In Dto packen
            Neo4JPropertyCollectionNodeDto dto = new Neo4JPropertyCollectionNodeDto()
            {
                Properties = properties
                    .Select(x => new Neo4JPropertyCollectionNodeDto.Property() { Key = x.Key, Value = x.Value })
                    .ToList(),
            };

            return dto;
        }
    }
}
