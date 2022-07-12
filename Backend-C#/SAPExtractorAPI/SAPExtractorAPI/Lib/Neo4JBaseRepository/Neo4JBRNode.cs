using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Security.AccessControl;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Ajax.Utilities;
using Neo4jClient;
using Neo4jClient.Cypher;
using Newtonsoft.Json;
using SAPExtractorAPI.Lib.Mapper;
using SAPExtractorAPI.Models.Helper;
using SAPExtractorAPI.Models.Neo4J;
using SAPExtractorAPI.Models.Neo4J.Node;
using SAPExtractorAPI.Models.Neo4J.NodeTypes;
using SAPExtractorAPI.Models.Neo4J.Relation;
using SAPExtractorAPI.Models.SAP;

namespace SAPExtractorAPI.Lib.Neo4JBaseRepository
{
    public abstract partial class Neo4JBaseRepository
    {
        #region GET

        /// <summary>
        /// Da die id von Neo4j sich aendern kann und deshalb nicht verwendet werden sollte
        /// wird hier eine eigene erstellt (durch hochzaehlen)
        /// </summary>
        protected async Task<long> GetNodeNextId()
        {
            //Maximale Id suchen
            long maxId = (await Client.Cypher.Match("(n)").Return((n) => n.As<Neo4JNodeDto>())
                .OrderByDescending("n.Id").Limit(1).ResultsAsync).Select(x => x.Id).FirstOrDefault();

            return maxId + 1;
        }

        public async Task<Neo4JNodeDto> GetNode(long id)
        {
            Neo4JNodeLabelsDto resNodes = (await Client.Cypher.Match("(component)").Where("component.Id = {idParam}")
                .WithParam("idParam", id)
                .Return(component => new Neo4JNodeLabelsDto()
                {
                    Component = component.As<Node<string>>(),
                    Labels = component.Labels()
                }).ResultsAsync).FirstOrDefault();

            if (resNodes != null)
            {
                return Neo4JNodeMapper.ToBusinessObject(resNodes);
            }

            return new Neo4JNodeDto();
        }

        public async Task<List<GenNode>> GetNodesByLabel(string label)
        {
            List<GenNode> result = new List<GenNode>();

            try
            {
                string nodeMatch = "(node:" + label + ")";

                var queryResult = await Client.Cypher.Match(nodeMatch)
                    .WithParam("type", "ObjectType")
                    .With("node, properties(node) as prop")
                    .Return((node, prop) => new GenNode
                    {
                        Id = node.Id(),
                        Labels = node.Labels(),
                        Properties = prop.As<Dictionary<string, string>>()
                    })
                    .ResultsAsync;

                result = queryResult.ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return result;
        }

        /// <summary>
        /// Returns all Object/Document Types
        /// </summary>
        /// <returns></returns>
        public async Task<List<NodeObjectTypeDto>> GetObjectTypes()
        {
            List<NodeObjectTypeDto> objectTypes = new List<NodeObjectTypeDto>();

            try
            {
                var queryResult = await Client.Cypher.Match("(node:ObjectType)-[e:IS_CREATED_IN]->(table)")
                    .Set("node.Neo4JId = id(node)")
                    .Set("node.Labels = labels(node)")
                    .Set("table.Neo4JId = id(table)")
                    .Set("table.Labels = labels(table)")
                    .Return((node, table) => new
                    {
                        ObjectType = node.As<NodeObjectTypeDto>(),
                        TableDtos = table.CollectAsDistinct<NodeTableDto>(),
                    })
                    .ResultsAsync;

                foreach (var result in queryResult)
                {
                    var objectType = result.ObjectType;
                    objectTypes.Add(objectType);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return objectTypes;
        }

        /// <summary>
        /// Returns all Object/Document Types that are related to Eventdata.
        /// </summary>
        /// <returns></returns>
        public async Task<List<NodeObjectTypeDto>> GetEventObjectTypes()
        {
            List<NodeObjectTypeDto> objectTypes = new List<NodeObjectTypeDto>();

            try
            {
                var queryResult = await Client.Cypher.Match("(node:ObjectType)-[e:IS_CREATED_IN]->(table:RECORDTABLE)")
                    .Set("node.Neo4JId = id(node)")
                    .Set("node.Labels = labels(node)")
                    .Set("table.Neo4JId = id(table)")
                    .Set("table.Labels = labels(table)")
                    .Return((node, table) => new
                    {
                        ObjectType = node.As<NodeObjectTypeDto>(),
                        TableDtos = table.CollectAsDistinct<NodeTableDto>(),
                    })
                    .ResultsAsync;

                foreach (var result in queryResult)
                {
                    var objectType = result.ObjectType;
                    objectTypes.Add(objectType);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return objectTypes;
        }


        public async Task<Neo4JGraphDto> GetObjectTypesNetwork()
        {
            List<Neo4JNodeDto> nodes = new List<Neo4JNodeDto>();
            List<Neo4JRelationDto> relations = new List<Neo4JRelationDto>();

            try
            {
                var queryResult = await Client.Cypher.Match("(node:ObjectType)-[e:IS_CREATED_IN]->(table)")
                    .Set("node.Neo4JId = id(node)")
                    .Set("node.Labels = labels(node)")
                    .Set("table.Neo4JId = id(table)")
                    .Set("table.Labels = labels(table)")
                    .Return((node, table) => new
                    {
                        ObjectType = node.As<NodeObjectTypeDto>(),
                        TableDtos = table.CollectAsDistinct<NodeTableDto>(),
                    })
                    .ResultsAsync;

                foreach (var result in queryResult)
                {
                    Neo4JNodeDto node = new Neo4JNodeDto
                    {
                        Id = Convert.ToInt64(result.ObjectType.Id),
                        Alias = result.ObjectType.Alias,
                        Name = result.ObjectType.Name,
                        Labels = result.ObjectType.Labels
                    };

                    nodes.Add(node);

                    foreach (var tableNode in result.TableDtos)
                    {
                        node = new Neo4JNodeDto
                        {
                            Id = Convert.ToInt64(tableNode.Id),
                            Alias = tableNode.Alias,
                            Name = tableNode.Name,
                            Labels = tableNode.Labels
                        };

                        Neo4JRelationDto relation = new Neo4JRelationDto
                        {
                            Node1Id = Convert.ToInt64(result.ObjectType.Id),
                            Node2Id = Convert.ToInt64(tableNode.Id),
                            RelationshipType = "IS_CREATED_IN"
                        };

                        nodes.Add(node);
                        relations.Add(relation);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Neo4JGraphDto graphDto = new Neo4JGraphDto();
            graphDto.Nodes = nodes;
            graphDto.Relations = relations;


            return graphDto;
        }


        public async Task<RelevantTableGraphDto> GetRelevantTablesForObjectType(string objectTypeName, int entrycount)
        {
            List<SAPTableNode> tables = new List<SAPTableNode>();
            List<NodeObjectTypeDto> dtoObjectTypes = new List<NodeObjectTypeDto>();
            List<Neo4JRelationDto> relations = new List<Neo4JRelationDto>();

            try
            {
                /*
                    MATCH (objectType:ObjectType) -[q:IS_CREATED_IN]->(masterRecTable:RECORDTABLE)
                    WHERE objectType.Name = $name

                    MATCH (masterRecTable)-[checkRel:IS_CHECKED_BY_KEY]-(connected)
                    WHERE connected.ENTRYCOUNT > $entries

                    MATCH (connected)-[objTypCon]-(objectTypeSec:ObjectType)

                    OPTIONAL MATCH (objectTypeSec)-[recSecRel]-(recordTableSec:RECORDTABLE)

                    RETURN objectType, masterRecTable, q, checkRel, connected, objTypCon, objectTypeSec, recSecRel, recordTableSec
                 
                 */
                var query = Client.Cypher
                    .Match("(objectType:ObjectType) -[q:IS_CREATED_IN]->(masterRecTable:RECORDTABLE)")
                    .Where("objectType.Name = '" + objectTypeName + "'")
                    .Match("(masterRecTable)-[checkRel:IS_CHECKED_BY_KEY]-(connected)")
                    .Where("connected.ENTRYCOUNT > $entrycount or connected:RECORDTABLE")
                    .WithParam("entrycount", entrycount)
                    .OptionalMatch("(connected)-[objTypCon]-(objectTypeSec:ObjectType)")
                    .OptionalMatch("(objectTypeSec)-[recSecRel]-(recordTableSec:RECORDTABLE)")
                    .OptionalMatch("(masterRecTable)-[recKeyRel:IS_DEFINED_BY_KEY]-(recKeyNode)")
                    .OptionalMatch("(connected)-[connKeyRel:IS_DEFINED_BY_KEY]-(connKeyNode)")
                    .OptionalMatch("(recordTableSec)-[secRecKeyRel:IS_DEFINED_BY_KEY]-(secRecKeyNode)")
                    .Return((objectType,
                        masterRecTable,
                        connected,
                        recordTableSec,
                        objectTypeSec,
                        q,
                        checkRel,
                        recSecRel,
                        objTypCon,
                        recKeyNode,
                        connKeyNode,
                        secRecKeyNode,
                        recKeyRel,
                        connKeyRel,
                        secRecKeyRel
                    ) => new
                    {
                        ObjectType = objectType.As<NodeObjectTypeDto>(),
                        TableDtos = masterRecTable.CollectAsDistinct<NodeTableDto>(),
                        ConnectedTables = connected.CollectAsDistinct<NodeTableDto>(),
                        RecordTableSec = recordTableSec.CollectAsDistinct<NodeTableDto>(),
                        ObjectTypeSec = objectTypeSec.CollectAsDistinct<NodeObjectTypeDto>(),

                        CreatedInRel = q.CollectAsDistinct<RelationDto>(),
                        CheckedByKeyRel = checkRel.CollectAsDistinct<RelationDto>(),
                        RecSecRel = recSecRel.CollectAsDistinct<RelationDto>(),
                        ObjectTypeSecRel = objTypCon.CollectAsDistinct<RelationDto>(),

                        //Keys
                        MasterRecTableKeys = recKeyNode.CollectAsDistinct<NodeKeyDto>(),
                        ConnectedTablesKeys = connKeyNode.CollectAsDistinct<NodeKeyDto>(),
                        SecRecTableKeys = secRecKeyNode.CollectAsDistinct<NodeKeyDto>(),

                        MasterRecTableKeyRels = recKeyRel.CollectAsDistinct<RelationDto>(),
                        ConnectedTableKeyRels = connKeyRel.CollectAsDistinct<RelationDto>(),
                        SecReCTableKeyRels = secRecKeyRel.CollectAsDistinct<RelationDto>()
                    });

                var queryResult = await query.ResultsAsync;


                foreach (var result in queryResult)
                {
                    Neo4JNodeDto node = new Neo4JNodeDto
                    {
                        Id = Convert.ToInt64(result.ObjectType.Neo4JId),
                        Alias = result.ObjectType.Alias,
                        Name = result.ObjectType.Name,
                        Labels = result.ObjectType.Labels
                    };
                    result.ObjectType.Id = result.ObjectType.Neo4JId;
                    dtoObjectTypes.Add(result.ObjectType);

                    foreach (var currTableNode in result.TableDtos)
                    {
                        SAPTableNode sapTableNode = new SAPTableNode();
                        currTableNode.Id = Convert.ToInt64(currTableNode.Neo4JId);

                        sapTableNode.TableNode = currTableNode;
                        foreach (NodeKeyDto masterRecTableKey in result.MasterRecTableKeys)
                        {
                            /*
                            var keyNode = new Neo4JNodeDto
                            {
                                Id = Convert.ToInt64(masterRecTableKey.Neo4JId),
                                Alias = masterRecTableKey.Alias,
                                Name = masterRecTableKey.Name,
                                Labels = masterRecTableKey.Labels
                            };
                            */

                            masterRecTableKey.Id = masterRecTableKey.Neo4JId.ToString();

                            sapTableNode.PrimaryKeyNodes.Add(masterRecTableKey);
                        }

                        Neo4JRelationDto relation = new Neo4JRelationDto
                        {
                            Node1Id = Convert.ToInt64(result.ObjectType.Neo4JId),
                            Node2Id = Convert.ToInt64(currTableNode.Neo4JId),
                            RelationshipType = "IS_CREATED_IN"
                        };

                        tables.Add(sapTableNode);
                        relations.Add(relation);
                    }

                    foreach (var currTableNode in result.ConnectedTables)
                    {
                        SAPTableNode sapTableNode = new SAPTableNode();

                        if (tables.FirstOrDefault(x => x.TableNode.Id == node.Id) == null)
                        {
                            currTableNode.Id = Convert.ToInt64(currTableNode.Neo4JId);

                            sapTableNode.TableNode = currTableNode;


                            var currPrimKeyRels =
                                result.ConnectedTableKeyRels.Where(x =>
                                    x.StartId.Equals(currTableNode.Neo4JId.ToString()));

                            foreach (var currPrimKeyRel in currPrimKeyRels)
                            {
                                var currKeyNode =
                                    result.ConnectedTablesKeys.FirstOrDefault(x =>
                                        x.Neo4JId.ToString().Equals(currPrimKeyRel.EndId));
                                if (currKeyNode != null)
                                {
                                    /*
                                    var keyNode = new Neo4JNodeDto
                                    {
                                        Id = Convert.ToInt64(currKeyNode.Neo4JId),
                                        Alias = currKeyNode.Alias,
                                        Name = currKeyNode.Name,
                                        Labels = currKeyNode.Labels
                                    };
                                    */

                                    currKeyNode.Id = currKeyNode.Neo4JId.ToString();

                                    sapTableNode.PrimaryKeyNodes.Add(currKeyNode);
                                }
                            }

                            tables.Add(sapTableNode);
                        }
                    }

                    foreach (var currTableNode in result.RecordTableSec)
                    {
                        SAPTableNode sapTableNode = new SAPTableNode();

                        if (tables.FirstOrDefault(x => x.TableNode.Id == node.Id) == null)
                        {
                            currTableNode.Id = Convert.ToInt64(currTableNode.Neo4JId);

                            sapTableNode.TableNode = currTableNode;

                            var currPrimKeyRels = result.SecReCTableKeyRels.Where(x =>
                                x.StartId.Equals(currTableNode.Neo4JId.ToString()));

                            foreach (var currPrimKeyRel in currPrimKeyRels)
                            {
                                var currKeyNode =
                                    result.SecRecTableKeys.FirstOrDefault(x =>
                                        x.Neo4JId.ToString().Equals(currPrimKeyRel.EndId));
                                if (currKeyNode != null)
                                {
                                    /*
                                    var keyNode = new Neo4JNodeDto
                                    {
                                        Id = Convert.ToInt64(currKeyNode.Neo4JId),
                                        Alias = currKeyNode.Alias,
                                        Name = currKeyNode.Name,
                                        Labels = currKeyNode.Labels
                                    };
                                    */

                                    currKeyNode.Id = currKeyNode.Neo4JId.ToString();

                                    sapTableNode.PrimaryKeyNodes.Add(currKeyNode);
                                }
                            }

                            tables.Add(sapTableNode);
                        }
                    }

                    foreach (var objectTypeNode in result.ObjectTypeSec)
                    {
                        Neo4JNodeDto objectTypeSecNode = new Neo4JNodeDto
                        {
                            Id = Convert.ToInt64(objectTypeNode.Neo4JId),
                            Alias = objectTypeNode.Alias,
                            Name = objectTypeNode.Name,
                            Labels = objectTypeNode.Labels
                        };

                        objectTypeNode.Id = objectTypeNode.Neo4JId;

                        if (dtoObjectTypes.FirstOrDefault(x => x.Id.Equals(objectTypeNode.Id)) == null)
                        {
                            dtoObjectTypes.Add(objectTypeNode);

                            var currRels = result.ObjectTypeSecRel.Where(x =>
                                x.StartId.Equals(objectTypeNode.Neo4JId.ToString()));

                            foreach (var currRelation in currRels)
                            {
                                Neo4JRelationDto relation = new Neo4JRelationDto
                                {
                                    Node1Id = Convert.ToInt64(currRelation.StartId),
                                    Node2Id = Convert.ToInt64(currRelation.EndId),
                                    RelationshipType = "IS_CREATED_IN"
                                };

                                relations.Add(relation);
                            }
                        }
                    }

                    foreach (RelationDto recSecRelation in result.RecSecRel)
                    {
                        Neo4JRelationDto relation = new Neo4JRelationDto
                        {
                            Node1Id = Convert.ToInt64(recSecRelation.StartId),
                            Node2Id = Convert.ToInt64(recSecRelation.EndId),
                            RelationshipType = "IS_CREATED_IN"
                        };

                        var foundRel = relations.FirstOrDefault(x =>
                            x.Node1Id == relation.Node1Id && x.Node2Id == relation.Node2Id);
                        if (foundRel == null)
                        {
                            relations.Add(relation);
                        }
                    }

                    foreach (var relationDto in result.CheckedByKeyRel)
                    {
                        Neo4JRelationDto relation = new Neo4JRelationDto
                        {
                            Node1Id = Convert.ToInt64(relationDto.StartId),
                            Node2Id = Convert.ToInt64(relationDto.EndId),
                            RelationshipType = "IS_CHECKED_BY_KEY"
                        };

                        var foundRel = relations.FirstOrDefault(x =>
                            x.Node1Id == relation.Node1Id && x.Node2Id == relation.Node2Id);
                        if (foundRel == null)
                        {
                            relations.Add(relation);
                        }
                    }

                    foreach (var relationDto in result.MasterRecTableKeyRels)
                    {
                        Neo4JRelationDto relation = new Neo4JRelationDto
                        {
                            Node1Id = Convert.ToInt64(relationDto.StartId),
                            Node2Id = Convert.ToInt64(relationDto.EndId),
                            RelationshipType = "IS_DEFINED_BY_KEY"
                        };

                        var foundRel = relations.FirstOrDefault(x =>
                            x.Node1Id == relation.Node1Id && x.Node2Id == relation.Node2Id);
                        if (foundRel == null)
                        {
                            relations.Add(relation);
                        }
                    }


                    foreach (var relationDto in result.ConnectedTableKeyRels)
                    {
                        Neo4JRelationDto relation = new Neo4JRelationDto
                        {
                            Node1Id = Convert.ToInt64(relationDto.StartId),
                            Node2Id = Convert.ToInt64(relationDto.EndId),
                            RelationshipType = "IS_DEFINED_BY_KEY"
                        };

                        var foundRel = relations.FirstOrDefault(x =>
                            x.Node1Id == relation.Node1Id && x.Node2Id == relation.Node2Id);
                        if (foundRel == null)
                        {
                            relations.Add(relation);
                        }
                    }

                    foreach (var relationDto in result.SecReCTableKeyRels)
                    {
                        Neo4JRelationDto relation = new Neo4JRelationDto
                        {
                            Node1Id = Convert.ToInt64(relationDto.StartId),
                            Node2Id = Convert.ToInt64(relationDto.EndId),
                            RelationshipType = "IS_DEFINED_BY_KEY"
                        };

                        var foundRel = relations.FirstOrDefault(x =>
                            x.Node1Id == relation.Node1Id && x.Node2Id == relation.Node2Id);
                        if (foundRel == null)
                        {
                            relations.Add(relation);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            RelevantTableGraphDto graphDto = new RelevantTableGraphDto();
            graphDto.Tables = tables;
            graphDto.ObjectTypes = dtoObjectTypes;
            graphDto.Relations = relations;


            return graphDto;
        }

        /// <summary>
        /// Returns the given object type together with all teh tables that are used to create entries for that object type.
        /// </summary>
        /// <param name="objectTypeName"></param>
        /// <returns></returns>
        public async Task<Neo4JGraphDto> GetObjectTypesNetwork(string objectTypeName)
        {
            List<Neo4JNodeDto> nodes = new List<Neo4JNodeDto>();
            List<Neo4JRelationDto> relations = new List<Neo4JRelationDto>();

            try
            {
                var queryResult = await Client.Cypher.Match("(node:ObjectType)-[e:IS_CREATED_IN]->(table)")
                    .Where("node.Name = '" + objectTypeName + "'")
                    .Set("node.Neo4JId = id(node)")
                    .Set("node.Labels = labels(node)")
                    .Set("table.Neo4JId = id(table)")
                    .Set("table.Labels = labels(table)")
                    .Return((node, table) => new
                    {
                        ObjectType = node.As<NodeObjectTypeDto>(),
                        TableDtos = table.CollectAsDistinct<NodeTableDto>(),
                    })
                    .ResultsAsync;

                foreach (var result in queryResult)
                {
                    Neo4JNodeDto node = new Neo4JNodeDto
                    {
                        Id = Convert.ToInt64(result.ObjectType.Id),
                        Alias = result.ObjectType.Alias,
                        Name = result.ObjectType.Name,
                        Labels = result.ObjectType.Labels
                    };

                    nodes.Add(node);

                    foreach (var tableNode in result.TableDtos)
                    {
                        node = new Neo4JNodeDto
                        {
                            Id = Convert.ToInt64(tableNode.Id),
                            Alias = tableNode.Alias,
                            Name = tableNode.Name,
                            Labels = tableNode.Labels
                        };

                        Neo4JRelationDto relation = new Neo4JRelationDto
                        {
                            Node1Id = Convert.ToInt64(result.ObjectType.Id),
                            Node2Id = Convert.ToInt64(tableNode.Id),
                            RelationshipType = "IS_CREATED_IN"
                        };

                        nodes.Add(node);
                        relations.Add(relation);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Neo4JGraphDto graphDto = new Neo4JGraphDto();
            graphDto.Nodes = nodes;
            graphDto.Relations = relations;


            return graphDto;
        }

        internal async Task<List<Neo4JNodeDto>> GetAllTableNodes()
        {
            List<Neo4JNodeDto> tables = new List<Neo4JNodeDto>();


            var queryResult = await Client.Cypher.Match("(table:TABLE)")
                .Return((table) => new
                {
                    Table = table.As<Neo4JNodeDto>(),
                })
                .ResultsAsync;

            foreach (var result in queryResult)
            {
                var table = result.Table;
                tables.Add(table);
            }

            return tables;
        }

        internal async Task<List<Neo4JNodeDto>> GetAllTCodeNodes()
        {
            List<Neo4JNodeDto> tables = new List<Neo4JNodeDto>();


            var queryResult = await Client.Cypher.Match("(table:TCODE)")
                .Return((table) => new
                {
                    Table = table.As<Neo4JNodeDto>(),
                })
                .ResultsAsync;

            foreach (var result in queryResult)
            {
                var table = result.Table;
                tables.Add(table);
            }

            return tables;
        }

        /// <summary>
        /// Returns the main table for the given tree.
        /// </summary>
        /// <param name="fieldName">The name of the field to search for </param>
        /// <returns></returns>
        public async Task<NodeTableDto> GetMainTableForKey(string fieldName)
        {
            NodeTableDto returnTableDto = null;

            try
            {
                /*
                    MATCH(key:KEY)
                    WHERE key.Name = $name

                    MATCH (key)<-[rel:IS_DEFINED_BY_KEY]-(table)
                    MATCH (table)-[IS_ENTITYTAB_OF]->(domain)
                    MATCH (key)-[IS_IN_DOMAIN]->(domain)

                    Return key, table, rel
                */

                // Looks if the current key has a entity table.
                var queryResult = await Client.Cypher.Match("(key:KEY)")
                    .Where("key.Name = $name")
                    .WithParam("name", fieldName)
                    .Match("(key)<-[rel:IS_DEFINED_BY_KEY]-(table)")
                    .Match("(table)-[IS_ENTITYTAB_OF]->(domain)")
                    .Match("(key)-[IS_IN_DOMAIN]->(domain)")
                    .Return((key, table) => new
                    {
                        KeyDto = key.As<NodeKeyDto>(),
                        TableDto = table.CollectAsDistinct<NodeTableDto>(),
                    })
                    .ResultsAsync;

                foreach (var result in queryResult)
                {
                    var foundTable = result.TableDto.FirstOrDefault();

                    returnTableDto = foundTable;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }


            return returnTableDto;
        }

        #endregion

        public async Task<List<Neo4JNodeDto>> AddNodesBulk(List<Neo4JNodeDto> nodes)
        {
            // Alte Knoten mit Namen loeschen
            List<string> names = nodes.Select(x => x.Name).Distinct().ToList();

            List<Neo4JNodeDto> retNodes = new List<Neo4JNodeDto>();

            foreach (string name in names)
            {
                var node = nodes.Where(x => x.Name.Equals(name)).First();
                if (node != null)
                {
                    nodes.Remove(node);
                    retNodes.Add(node);
                }
            }

            names = names.Select(x => x.Replace("\"", "\\\"")).ToList();


            //Alle Knoten mit gleichem Namen loeschen
            string namesString = "[\"" + string.Join("\",\"", names) + "\"]";

            var cypher = Client.Cypher.Match("(n)").Where(string.Format("n.Name in {0}", namesString))
                .DetachDelete("n");
            await cypher.ExecuteWithoutResultsAsync();

            // Neue Query fuer hinzufuegen

            long nextNodeId = await GetNodeNextId();

            // Nach Label gruppieren
            var labelGroups = retNodes.GroupBy(x => "`" + string.Join("`:`", x.Labels) + "`").ToList();
            foreach (IGrouping<string, Neo4JNodeDto> labelgroup in labelGroups)
            {
                List<Neo4JNodeDto> groupNodes = labelgroup.ToList();

                List<Dictionary<string, object>> nodeDictionaries = new List<Dictionary<string, object>>();

                foreach (Neo4JNodeDto groupNode in groupNodes)
                {
                    groupNode.Id = nextNodeId;
                    nextNodeId++;

                    // Eigenschaften fuer Knoten erstellen
                    Dictionary<string, object> nodeProps = new Dictionary<string, object>()
                    {
                        {"Id", groupNode.Id},
                        {"Name", groupNode.Name},
                        {"Alias", groupNode.Alias}
                    };
                    // Dynamische Eigenschaften erstellen
                    foreach (Neo4jNodePropertyDto props in groupNode.Properties)
                    {
                        nodeProps.Add(props.PropertyName, props.Value);
                    }

                    nodeDictionaries.Add(nodeProps);
                }

                //Query ausfuehren und Knoten hinzufuegen
                ICypherFluentQuery cmd = Client.Cypher
                    .Unwind(nodeDictionaries, "node")
                    .Create(string.Format("(n:{0})", labelgroup.Key))
                    .Set("n = node");

                await cmd.ExecuteWithoutResultsAsync();
            }

            return retNodes;
        }
    }
}