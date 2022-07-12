using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Neo4jClient;
using Newtonsoft.Json;
using SAPExtractorAPI.Models.Neo4J;

namespace SAPExtractorAPI.Lib.Mapper
{
    public class Neo4JNodeMapper : BaseNeo4JNodeMapper
    {
        public static Neo4JNodeDto ToBusinessObject(Neo4JNodeLabelsDto node)
        {

            Neo4JPropertyCollectionNodeDto propertyCollectionNode = ToBusinessObject(node.Component);

            Neo4JNodeDto dto = new Neo4JNodeDto() { };

            //Namen Eigenschaft raussuchen, da diese direkt mit der Componente gespeichert wird
            Neo4JPropertyCollectionNodeDto.Property nameProperty =
                propertyCollectionNode.Properties.FirstOrDefault(x => x.Key.ToLower().Equals("name"));

            //Id Eigenschaft raussuchen, da diese direkt mit der Componente gespeichert wird
            Neo4JPropertyCollectionNodeDto.Property idProperty =
                propertyCollectionNode.Properties.FirstOrDefault(x => x.Key.ToLower().Equals("id"));

            dto.referenceId = -1;
            //Id Property
            if (idProperty != null)
            {
                //Eigenschaft aus Liste entfernen 
                propertyCollectionNode.Properties.Remove(idProperty);

                long id;
                if (long.TryParse(idProperty.Value, out id))
                {
                    dto.Id = id;
                }
            }

            if (nameProperty != null)
            {
                //Eigenschaft aus Liste entfernen 
                propertyCollectionNode.Properties.Remove(nameProperty);
                dto.Name = nameProperty.Value;
            }

            //Den Rest der Eigenschaften setzen
            dto.Properties =
                propertyCollectionNode.Properties.Select(
                    x => new Neo4jNodePropertyDto()
                    {
                        PropertyName = x.Key,
                        Value = x.Value,
                        NodeId = dto.Id
                    }).OrderBy(x => x.PropertyName).ToList();


            //Labels
            dto.Labels = node.Labels.ToList();


            return dto;

        }

        public static Neo4JNodeDto ToBusinessObjectWithReferenceId(Neo4JNodeLabelsDto node, long referenceId)
        {

            Neo4JPropertyCollectionNodeDto propertyCollectionNode = ToBusinessObject(node.Component);

            Neo4JNodeDto dto = new Neo4JNodeDto() { };

            //Namen Eigenschaft raussuchen, da diese direkt mit der Componente gespeichert wird
            Neo4JPropertyCollectionNodeDto.Property nameProperty =
                propertyCollectionNode.Properties.FirstOrDefault(x => x.Key.ToLower().Equals("name"));

            //Id Eigenschaft raussuchen, da diese direkt mit der Componente gespeichert wird
            Neo4JPropertyCollectionNodeDto.Property idProperty =
                propertyCollectionNode.Properties.FirstOrDefault(x => x.Key.ToLower().Equals("id"));


            Neo4JPropertyCollectionNodeDto.Property xProperty =
                propertyCollectionNode.Properties.FirstOrDefault(x => x.Key.ToLower().Equals("x"));

            Neo4JPropertyCollectionNodeDto.Property yProperty =
                propertyCollectionNode.Properties.FirstOrDefault(x => x.Key.ToLower().Equals("y"));

            dto.referenceId = referenceId;

            //Id Property
            if (idProperty != null)
            {
                //Eigenschaft aus Liste entfernen 
                propertyCollectionNode.Properties.Remove(idProperty);

                long id;
                if (long.TryParse(idProperty.Value, out id))
                {
                    dto.Id = id;
                }
            }

            if (nameProperty != null)
            {
                //Eigenschaft aus Liste entfernen 
                propertyCollectionNode.Properties.Remove(nameProperty);
                dto.Name = nameProperty.Value;
            }

            //Den Rest der Eigenschaften setzen
            dto.Properties =
                propertyCollectionNode.Properties.Select(
                    x => new Neo4jNodePropertyDto()
                    {
                        PropertyName = x.Key,
                        Value = x.Value,
                        NodeId = dto.Id
                    }).OrderBy(x => x.PropertyName).ToList();


            //Labels
            dto.Labels = node.Labels.ToList();


            return dto;

        }
    }
}
