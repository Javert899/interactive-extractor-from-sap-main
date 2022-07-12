using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Neo4jClient;
using Neo4jClient.Cypher;
using SAPExtractorAPI.Lib.Mapper;
using SAPExtractorAPI.Models.Helper;
using SAPExtractorAPI.Models.Neo4J;
using SAPExtractorAPI.Models.Neo4J.Relation;

namespace SAPExtractorAPI.Lib.Neo4JBaseRepository
{
    public abstract partial class Neo4JBaseRepository
    {
        protected async Task<long> GetNextRelationId()
        {
            //Maximale Id suchen
            long maxId = (await Client.Cypher.Match("(n)-[r]-(n2)").Return((r) => r.As<Neo4JRelationDto>())
                .OrderByDescending("r.Id").Limit(1).ResultsAsync).Select(x => x.Id).FirstOrDefault();

            return maxId + 1;
        }

        private ICypherFluentQuery GetRelationByIdQuery(long id)
        {
            return Client.Cypher.Match("(n1)-[r]-(n2)").Where("r.Id = {relationId}")
                .WithParam("relationId", id);
        }

        public async Task<Neo4JRelationDto> GetRelation(long relationId)
        {
            return ( await GetRelationByIdQuery(relationId).Return((n1, n2, r) => new
                    Neo4JRawRelationDto
                    {
                        Node1Id = n1.As<Neo4JNodeDto>().Id,
                        Node2Id = n2.As<Neo4JNodeDto>().Id,
                        Relation = r.As<RelationshipInstance<Dictionary<string, string>>>(),
                        RelationshipType = r.Type()
                    })
                .ResultsAsync).Select(Neo4JRelationMapper.ToBusinessObject).FirstOrDefault();
        }


        public async Task<List<Neo4JRelationDto>> AddRelationsBulk(List<Neo4JRelationDto> relations, bool directed = false)
        {
            long nextRelationId = await GetNextRelationId();

            //Ids vergeben
            foreach (var item in relations)
            {
                item.Id = nextRelationId;
                nextRelationId++;
            }

            //Alle KnotenIds suchen
            List<long> nodeIdsToAdd = relations.Select(x => x.Node1Id).ToList();
            nodeIdsToAdd.AddRange(relations.Select(x => x.Node2Id));
            nodeIdsToAdd = nodeIdsToAdd.Distinct().ToList();

            // Dictionary erstellen mit allen eigen erstellten Ids und den Neo4JIds 
            // Neo4JIds beschleunigen die Suche, da es ewig dauert anhand der Eigenschaft "Id" zu suchen
            Dictionary<long, long> allIdsDict = (await Client.Cypher
                .Match("(n)")
                .Return((n) => new { Neo4JId = n.Id(), Id = n.As<NodeIdHelperDto>().Id })
                .ResultsAsync).ToDictionary(x => x.Id, x => x.Neo4JId);


            //Types setzten falls nicht vorhanden
            relations.Where(x => string.IsNullOrEmpty(x.RelationshipType)).ToList().ForEach(x => x.RelationshipType = DefaultRelationType);

            // Neo4JId zu den Eigenschaften "Id"s suchen und in die Relations setzen
            //Die Relations die fuer die Erstellung genutzt werden
            List<AddRelationHelperDto> availableRelations = new List<AddRelationHelperDto>();
            //Einmal alle Relations die danach zurueckgegeben werden
            List<Neo4JRelationDto> ret = new List<Neo4JRelationDto>();

            foreach (var item in relations)
            {
                long neo4JId = -1;
                long neo4JId2 = -1;

                if (allIdsDict.TryGetValue(item.Node1Id, out neo4JId) && allIdsDict.TryGetValue(item.Node2Id, out neo4JId2))
                {
                    Dictionary<string, object> propDict = new Dictionary<string, object>();
                    foreach (Neo4JRelationPropertyDto property in item.Properties)
                    {
                        propDict.Add(property.PropertyName, property.Value);
                    }

                    availableRelations.Add(new AddRelationHelperDto()
                    {
                        Neo4JId1 = neo4JId,
                        Neo4JId2 = neo4JId2,
                        Id = item.Id,
                        RelationshipType = item.RelationshipType,
                        Propertys = propDict
                    });

                    ret.Add(item);
                }
            }
            // Nach Typ gruppieren
            List<IGrouping<string, AddRelationHelperDto>> typeGroups = availableRelations.GroupBy(x => x.RelationshipType).ToList();
            foreach (IGrouping<string, AddRelationHelperDto> typeGroup in typeGroups)
            {
                //Alle Verbindungen erstellen
                await Client.Cypher
               .Unwind(typeGroup.ToList(), "relationship")
               .Match("(grant)", "(target)")
               .Where("id(grant) = relationship.Neo4JId1 and id(target) = relationship.Neo4JId2")
               .Create(string.Format("(grant)-[r:`{0}`]->(target)", typeGroup.Key))
               .Set("r = relationship.Propertys")
               .ExecuteWithoutResultsAsync();
            }

            return ret;
        }
        public async Task<List<Neo4JRelationDto>> MergeRelationsBulk(List<Neo4JRelationDto> relations, bool directed = false)
        {
            long nextRelationId = await GetNextRelationId();

            //Ids vergeben
            foreach (var item in relations)
            {
                item.Id = nextRelationId;
                nextRelationId++;
            }

            //Alle KnotenIds suchen
            List<long> nodeIdsToAdd = relations.Select(x => x.Node1Id).ToList();
            nodeIdsToAdd.AddRange(relations.Select(x => x.Node2Id));
            nodeIdsToAdd = nodeIdsToAdd.Distinct().ToList();

            // Dictionary erstellen mit allen eigen erstellten Ids und den Neo4JIds 
            // Neo4JIds beschleunigen die Suche, da es ewig dauert anhand der Eigenschaft "Id" zu suchen
            Dictionary<long, long> allIdsDict = (await Client.Cypher
                .Match("(n)")
                .Return((n) => new { Neo4JId = n.Id(), Id = n.As<NodeIdHelperDto>().Id })
                .ResultsAsync).ToDictionary(x => x.Id, x => x.Neo4JId);


            //Types setzten falls nicht vorhanden
            relations.Where(x => string.IsNullOrEmpty(x.RelationshipType)).ToList().ForEach(x => x.RelationshipType = DefaultRelationType);

            // Neo4JId zu den Eigenschaften "Id"s suchen und in die Relations setzen
            //Die Relations die fuer die Erstellung genutzt werden
            List<AddRelationHelperDto> availableRelations = new List<AddRelationHelperDto>();
            //Einmal alle Relations die danach zurueckgegeben werden
            List<Neo4JRelationDto> ret = new List<Neo4JRelationDto>();

            foreach (var item in relations)
            {
                long neo4JId = -1;
                long neo4JId2 = -1;

                if (allIdsDict.TryGetValue(item.Node1Id, out neo4JId) && allIdsDict.TryGetValue(item.Node2Id, out neo4JId2))
                {
                    availableRelations.Add(new AddRelationHelperDto()
                    {
                        Neo4JId1 = neo4JId,
                        Neo4JId2 = neo4JId2,
                        Id = item.Id,
                        RelationshipType = item.RelationshipType

                    });

                    ret.Add(item);
                }
            }
            // Nach Typ gruppieren
            List<IGrouping<string, AddRelationHelperDto>> typeGroups = availableRelations.GroupBy(x => x.RelationshipType).ToList();
            foreach (IGrouping<string, AddRelationHelperDto> typeGroup in typeGroups)
            {
                //Alle Verbindungen erstellen
                await Client.Cypher
               .Unwind(typeGroup.ToList(), "relationship")
               .Match("(grant)", "(target)")
               .Where("id(grant) = relationship.Neo4JId1 and id(target) = relationship.Neo4JId2")
               .Merge(string.Format("(grant)-[r:`{0}`]->(target)", typeGroup.Key))
               .Set("r.Id = relationship.Id")
               .ExecuteWithoutResultsAsync();
            }

            return ret;
        }
        
        #region helperClasses
        private class AddRelationHelperDto
        {
            /// <summary>
            /// Interne Neo4JRelationId
            /// </summary>
            public long Id { get; set; }
            /// <summary>
            /// Id der Quelle
            /// </summary>
            public long Neo4JId1 { get; set; }
            /// <summary>
            /// Id des Ziels
            /// </summary>
            public long Neo4JId2 { get; set; }
            ///// <summary>
            ///// Relationship Typ 
            ///// </summary>
            public string RelationshipType { get; set; }

            public Dictionary<string,object> Propertys { get; set; }
        }

        private class NodeIdHelperDto
        {
            public long Id { get; set; }
        }
        #endregion

    }
}