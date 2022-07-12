using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SAPExtractorAPI.Models.Neo4J;
using SAPExtractorAPI.Models.Neo4J.Relation;

namespace SAPExtractorAPI.Lib.Mapper
{
    public class Neo4JRelationMapper
    {

        public static Neo4JRelationDto ToBusinessObject(Neo4JRawRelationDto entity)
        {
            Neo4JRelationDto relationDto = new Neo4JRelationDto
            {
                Node1Id = entity.Node1Id,
                Node2Id = entity.Node2Id,
                RelationshipType = entity.RelationshipType

            };

            List<Neo4JRelationPropertyDto> properties = entity.Relation.Data
                .Select(y => new Neo4JRelationPropertyDto() { Value = y.Value, PropertyName = y.Key }).ToList();

            Neo4JRelationPropertyDto propId = properties.FirstOrDefault(x => x.PropertyName.Equals("Id"));
            long id;

            if (propId != null && long.TryParse(propId.Value.ToString(), out id))
            {
                relationDto.Id = id;
                properties.Remove(propId);

                for (var i = 0; i < properties.Count; i++)
                {
                    Neo4JRelationPropertyDto property = properties[i];
                    property.RelationId = id;
                    //property.OrderNo = i;
                }
            }


            relationDto.Properties = properties;


            return relationDto;

        }
    }
}
