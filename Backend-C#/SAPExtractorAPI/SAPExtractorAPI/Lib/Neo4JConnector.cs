using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI.WebControls;
using SAPExtractorAPI.Models.Neo4J;
using SAPExtractorAPI.Models.Neo4J.NodeTypes;
using SAPExtractorAPI.Models.Neo4J.Relation;
using SAPExtractorAPI.Models.SAP;

namespace SAPExtractorAPI.Lib
{
    public class Neo4JConnector
    {
        string neo4jUrl = ConfigurationManager.AppSettings["neodburi"];
        string neo4jpass = ConfigurationManager.AppSettings["neodbpass"];
        string neo4juser = ConfigurationManager.AppSettings["neodbuser"];



        private Neo4JBaseRepository.Neo4JBaseRepository neo4Repository;

        public Neo4JConnector()
        {
            neo4Repository = new Neo4JRepository.Neo4JRepository(neo4jUrl, neo4juser, neo4jpass);
        }


        /// <summary>
        /// Checks if the connection to Neo4J is still open.
        /// </summary>
        private void CheckConnection()
        {
            bool connected = neo4Repository.IsConnected();
            this.neo4Repository.Connect(neo4jUrl, neo4juser, neo4jpass);

            if (!connected)
            {
                neo4Repository = new Neo4JRepository.Neo4JRepository(neo4jUrl, neo4juser, neo4jpass);
            }
        }

        #region Get Data

        public async Task<List<NodeObjectTypeDto>> GetEventObjectTypes()
        {
            this.CheckConnection();

            var nodes = await neo4Repository.GetEventObjectTypes();

            return nodes;
        }

        public async Task<Neo4JGraphDto> GetObjectTypesNetwork()
        {
            this.CheckConnection();

            var nodes = await neo4Repository.GetObjectTypesNetwork();
            
            return nodes;
        }

        public async Task<Neo4JGraphDto> GetObjectTypesNetwork(string objectTypeName)
        {
            this.CheckConnection();

            var nodes = await neo4Repository.GetObjectTypesNetwork(objectTypeName);

            return nodes;
        }


        /// <summary>
        /// Returns the relevant Tables for a given Object Type.
        /// </summary>
        /// <param name="objectTypeName"></param>
        /// <param name="entrycount"></param>
        /// <returns></returns>
        internal async Task<RelevantTableGraphDto> GetRelevantTablesForObjectType(string objectTypeName, int entrycount)
        {
            this.CheckConnection();

            var nodes = await neo4Repository.GetRelevantTablesForObjectType(objectTypeName, entrycount);

            return nodes;
        }


        /// <summary>
        /// Returns the main table for a given key.
        /// </summary>
        /// <param name="fieldDto"></param>
        /// <param name="tables"></param>
        /// <returns></returns>
        internal async Task<NodeTableDto> GetMainTableForKey(NodeKeyDto fieldDto, List<SAPTableNode> tables)
        {
            this.CheckConnection();
            if (fieldDto != null && tables != null)
            {
                var table = await neo4Repository.GetMainTableForKey(fieldDto.Name);
                return table;
            }
            else
            {
                return null;
            }
        }


        internal async Task<List<Neo4JNodeDto>> GetAllTables()
        {
            this.CheckConnection();

            var tables = await neo4Repository.GetAllTableNodes();


            return tables;
        }

        internal async Task<List<Neo4JNodeDto>> GetAllTCodes()
        {
            this.CheckConnection();

            var tables = await neo4Repository.GetAllTCodeNodes();


            return tables;
        }

        #endregion

        /// <summary>
        /// Extends the already created Relations by relations like IS_CHECKED_BY_KEY and adds a few parameters to relations.
        /// Automatically identifies Record Tables
        /// </summary>
        /// <returns></returns>
        public async Task CreateExtendedSAPGraph()
        {
            bool connected = neo4Repository.IsConnected();
            this.neo4Repository.Connect(neo4jUrl, neo4juser, neo4jpass);

            if (!connected)
            {
                neo4Repository = new Neo4JRepository.Neo4JRepository(neo4jUrl, neo4juser, neo4jpass);
            }


            // Set Checked_By_Key Tables Connected By Key
            var query = neo4Repository.Client.Cypher.Match("(n)-[p:IS_DEFINED_BY_KEY]->(key)")
                .Match("(n)<-[q:IS_CREATED_IN]-(objectType)")
                .Match("(n)<-[c:IS_CHECKED_BY]-(checkTable) where c.Field = key.FIELDNAME")
                .Create("(n)<-[r2:IS_CHECKED_BY_KEY]-(checkTable)")
                .Set("r2 = c");

            await this.neo4Repository.ExecuteQueryWithoutResult(query);

            //Identify Event Date Fields
            query = neo4Repository.Client.Cypher
                .Match("(table:TABLE)-[]->(resourceField:RESOURCEFIELD)")
                .Where("NOT (table)-[:IS_DEFINED_BY_EVENTDATEFIELD]->()")
                .Match("(table)-[i:IS_DEFINED_BY_DATEFIELD]->(F:DATEFIELD)")
                .With("Min(F.Position) as Minimum,table")
                .Match("(table)-[rel]-(F2)")
                .Where("F2.Position = Minimum and F2.Position < 15")
                .Create("(table)-[r2:IS_DEFINED_BY_EVENTDATEFIELD]->(F2)")
                .Set("F2:SAP:PROPERTYFIELD:EVENTDATE:DATEFIELD");
            await this.neo4Repository.ExecuteQueryWithoutResult(query);

            // Identify Record Tables By Pattern Search
            query = neo4Repository.Client.Cypher
                .Match("(table:TABLE)")
                .Match("(table)-[r:IS_DEFINED_BY_EVENTDATEFIELD]->(eventDate)")
                .Match("(table)-[s:IS_DEFINED_BY_RESOURCEFIELD]->(eventResource)")
                .OptionalMatch("(table)-[:IS_DEFINED_BY_KEY]->(keyNode)<-[:IS_DEFINED_BY_KEY]-()<-[:IS_CHECKED_BY_KEY]-(table)")
                .Match("(table)")
                .Where("NOT (table)-[:IS_DEFINED_BY_KEY]->(keyNode)<-[:IS_DEFINED_BY_KEY]-()<-[:IS_CHECKED_BY_KEY]-(table)")
                .Set("table:TABLE:RECORDTABLE");
            await this.neo4Repository.ExecuteQueryWithoutResult(query);

            // Add the start and end node id for all Relationships for easier retrival.
            query = neo4Repository.Client.Cypher
                .Match("(start)-[q]->(end)")
                .Set("q.StartId = id(startNode(q))")
                .Set("q.EndId = id(endNode(q))");
            await this.neo4Repository.ExecuteQueryWithoutResult(query);

            // Add the internal Neo4J ID and  the labels to the node itself 
            query = neo4Repository.Client.Cypher
                .Match("(node)")
                .Set("node.Neo4JId = id(node)")
                .Set("node.Labels = labels(node)");
            await this.neo4Repository.ExecuteQueryWithoutResult(query);
        }





        #region Import Data

        public async Task<List<Neo4JNodeDto>> ImportNodesBulk(List<Neo4JNodeDto> nodes)
        {
            bool connected = neo4Repository.IsConnected();
            this.neo4Repository.Connect(neo4jUrl, neo4juser, neo4jpass);

            if (!connected)
            {
                neo4Repository = new Neo4JRepository.Neo4JRepository(neo4jUrl, neo4juser, neo4jpass);
            }
            
            return await neo4Repository.AddNodesBulk(nodes);
        }

        public async Task ImportSAPRelations(List<SAPTableRelationDto> sapRelationDtos, List<Neo4JNodeDto> fromNodes, List<Neo4JNodeDto> toNodes)
        {
            List<Neo4JRelationDto> neo4JRelations = new List<Neo4JRelationDto>();

            foreach (SAPTableRelationDto keyFieldsRelation in sapRelationDtos)
            {
                string fromTable = keyFieldsRelation.Node1Name;
                string toTable = keyFieldsRelation.Node2Name;
                if (!fromTable.Equals(toTable))
                {
                    Neo4JNodeDto fromNode = fromNodes.FirstOrDefault(x => x.Name.Equals(fromTable));
                    Neo4JNodeDto toNode = toNodes.FirstOrDefault(x => x.Name.Equals(toTable));

                    if (fromNode != null && toNode != null)
                    {
                        Neo4JRelationDto newRelation = new Neo4JRelationDto();
                        newRelation.Node1Id = fromNode.Id;
                        newRelation.Node2Id = toNode.Id;
                        newRelation.RelationshipType = keyFieldsRelation.RelationshipType;
                        newRelation.Properties = new List<Neo4JRelationPropertyDto>();

                        foreach (SAPRelationProperty edgeRelationProperty in keyFieldsRelation.RelationProperties)
                        {
                            Neo4JRelationPropertyDto neo4JRelationPropertyDto = new Neo4JRelationPropertyDto();

                            neo4JRelationPropertyDto.PropertyName = edgeRelationProperty.PropertyName;
                            neo4JRelationPropertyDto.Value = edgeRelationProperty.PropertyValue;

                            newRelation.Properties.Add(neo4JRelationPropertyDto);
                        }

                        neo4JRelations.Add(newRelation);
                    }
                }
            }
            int counter = 0;
            // Splitte die Datenmenge in Batches der Größe 1000 auf.
            while (counter < neo4JRelations.Count)
            {
                if (neo4JRelations.Count - counter < 20000)
                {
                    List<Neo4JRelationDto> currentRelationBatch = neo4JRelations.GetRange(counter, neo4JRelations.Count - counter);
                    await neo4Repository.AddRelationsBulk(currentRelationBatch);
                }
                else
                {
                    List<Neo4JRelationDto> currentRelationBatch = neo4JRelations.GetRange(counter, 20000);
                    await neo4Repository.AddRelationsBulk(currentRelationBatch);
                }
                counter = counter + 20000;
            }
            // await neo4Repository.AddRelationsBulk(neo4JRelations, true);
        }

        public async Task ImportSAPObjectTypes(SAPGraphDto objectTypes, List<Neo4JNodeDto> neo4JTableNodes)
        {


            // Importiere Objekttypen
            List<Neo4JNodeDto> sapObjectNodes = new List<Neo4JNodeDto>();

            foreach (SAPTableDto table in objectTypes.Expanded_tables)
            {
                Neo4JNodeDto neo4JNodeDto = new Neo4JNodeDto();
                neo4JNodeDto.Label = "SAP:Object:ObjectType";
                neo4JNodeDto.Name = table.TableName;
                neo4JNodeDto.Alias = table.TableName;
                foreach (SAPTableProperty tableTableProperty in table.TableProperties)
                {
                    neo4JNodeDto.AddProperty(tableTableProperty.PropertyName, tableTableProperty.PropertyValue);
                }

                sapObjectNodes.Add(neo4JNodeDto);
            }

            sapObjectNodes = await neo4Repository.AddNodesBulk(sapObjectNodes);


            List<Neo4JRelationDto> sapObjectRelations = new List<Neo4JRelationDto>();
            foreach (SAPTableRelationDto edge in objectTypes.Edges)
            {
                string fromTable = edge.Node1Name;
                string toTable = edge.Node2Name;
                if (!fromTable.Equals(toTable))
                {
                    // Verbindung geht von einem Objekttypen zu einer Tabelle
                    Neo4JNodeDto fromNode = sapObjectNodes.FirstOrDefault(x => x.Name.Equals(fromTable));
                    Neo4JNodeDto toNode = neo4JTableNodes.FirstOrDefault(x => x.Name.Equals(toTable));

                    if (fromNode != null && toNode != null)
                    {
                        Neo4JRelationDto newRelation = new Neo4JRelationDto();
                        newRelation.Node1Id = fromNode.Id;
                        newRelation.Node2Id = toNode.Id;
                        newRelation.RelationshipType = "IS_CREATED_IN";
                        newRelation.Properties = new List<Neo4JRelationPropertyDto>();
                        foreach (SAPRelationProperty edgeRelationProperty in edge.RelationProperties)
                        {
                            Neo4JRelationPropertyDto neo4JRelationPropertyDto = new Neo4JRelationPropertyDto();

                            neo4JRelationPropertyDto.PropertyName = edgeRelationProperty.PropertyName;
                            neo4JRelationPropertyDto.Value = edgeRelationProperty.PropertyValue;

                            newRelation.Properties.Add(neo4JRelationPropertyDto);
                        }

                        sapObjectRelations.Add(newRelation);
                    }
                }
            }
            await neo4Repository.AddRelationsBulk(sapObjectRelations, true);
        }

        public async Task ImportSAPApplicationClasses(List<Neo4JNodeDto> neo4JTableNodes, SAPNeo4JCompleteGraphDto sapNeo4JCompleteGraphDto)
        {
            bool connected = neo4Repository.IsConnected();
            this.neo4Repository.Connect(neo4jUrl, neo4juser, neo4jpass);

            if (!connected)
            {
                neo4Repository = new Neo4JRepository.Neo4JRepository(neo4jUrl, neo4juser, neo4jpass);
            }

            List<Neo4JNodeDto> neo4JAPPClassNodes = sapNeo4JCompleteGraphDto.TableNodes;
            neo4JAPPClassNodes = await neo4Repository.AddNodesBulk(neo4JAPPClassNodes);

            List<Neo4JRelationDto> neo4JTableRelations = new List<Neo4JRelationDto>();
            List<Neo4JRelationDto> neo4JTableKeyFieldsRelations = new List<Neo4JRelationDto>();

            foreach (SAPTableRelationDto edge in sapNeo4JCompleteGraphDto.TableEdges)
            {
                string fromTable = edge.Node1Name;
                string toTable = edge.Node2Name;
                if (!fromTable.Equals(toTable))
                {
                    Neo4JNodeDto fromNode = neo4JTableNodes.FirstOrDefault(x => x.Name.Equals(fromTable));
                    Neo4JNodeDto toNode = neo4JTableNodes.FirstOrDefault(x => x.Name.Equals(toTable));


                    if (edge.RelationshipType.Equals("IS_LOCATED_IN"))
                    {
                        fromNode = neo4JTableNodes.FirstOrDefault(x => x.Name.Equals(fromTable));
                        toNode = neo4JAPPClassNodes.FirstOrDefault(x => x.Name.Equals(toTable));
                    }
                    else if(edge.RelationshipType.Equals("IS_SUBMODULE_IN"))
                    {
                        fromNode = neo4JAPPClassNodes.FirstOrDefault(x => x.Name.Equals(fromTable));
                        toNode = neo4JAPPClassNodes.FirstOrDefault(x => x.Name.Equals(toTable));
                    }

                    if (fromNode != null && toNode != null)
                    {
                        Neo4JRelationDto newRelation = new Neo4JRelationDto();
                        newRelation.Node1Id = fromNode.Id;
                        newRelation.Node2Id = toNode.Id;
                        newRelation.RelationshipType = edge.RelationshipType;
                        newRelation.Properties = new List<Neo4JRelationPropertyDto>();

                        foreach (SAPRelationProperty edgeRelationProperty in edge.RelationProperties)
                        {
                            Neo4JRelationPropertyDto neo4JRelationPropertyDto = new Neo4JRelationPropertyDto();

                            neo4JRelationPropertyDto.PropertyName = edgeRelationProperty.PropertyName;
                            neo4JRelationPropertyDto.Value = edgeRelationProperty.PropertyValue;

                            newRelation.Properties.Add(neo4JRelationPropertyDto);
                        }

                        neo4JTableRelations.Add(newRelation);
                    }
                }
            }

            await neo4Repository.AddRelationsBulk(neo4JTableRelations, true);
        }




        #endregion
    }
}