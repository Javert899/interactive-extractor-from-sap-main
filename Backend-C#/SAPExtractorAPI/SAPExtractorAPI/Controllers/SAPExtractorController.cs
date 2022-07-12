using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using RestSharp;
using SAPExtractorAPI.Lib;
using SAPExtractorAPI.Lib.Neo4JBaseRepository;
using SAPExtractorAPI.Lib.Neo4JRepository;
using SAPExtractorAPI.Models.Helper;
using SAPExtractorAPI.Models.Neo4J;
using SAPExtractorAPI.Models.Neo4J.NodeTypes;
using SAPExtractorAPI.Models.Neo4J.Relation;
using SAPExtractorAPI.Models.SAP;


namespace SAPExtractorAPI.Controllers
{
    public class SAPExtractorController: ApiController
    {
        private string pythonAPIUrl = ConfigurationManager.AppSettings["pythonapirul"];

        // Load Connection Parameters
        string username = ConfigurationManager.AppSettings["oracle_username"];
        string password = ConfigurationManager.AppSettings["oracle_password"];
        string oracle_hostname = ConfigurationManager.AppSettings["oracle_hostname"];
        string oracle_port = ConfigurationManager.AppSettings["oracle_port"];
        string oracle_sid = ConfigurationManager.AppSettings["oracle_sid"];

        private OracleConnector oracleConnector = new OracleConnector();
        private Neo4JConnector neo4JConnector = new Neo4JConnector();


        public SAPExtractorController()
        {
        }


        [HttpGet]
        [Route("api/SAPExtractor/GetServerVersion")]
        public string GetServerVersion()
        {
            return "1.2";
        }

        #region SAP Extractor

        [HttpGet]
        [Route("api/SAPExtractor/GetObjectTypes")]
        public async Task<List<NodeObjectTypeDto>> GetObjectTypes()
        {
            var objectTypes = await this.neo4JConnector.GetEventObjectTypes();

            return objectTypes;
        }

        [HttpGet]
        [Route("api/SAPExtractor/GetEventObjectTypes")]
        public async Task<List<NodeObjectTypeDto>> GetEventObjectTypes()
        {
            var objectTypes = await this.neo4JConnector.GetEventObjectTypes();

            return objectTypes;
        }


        [HttpGet]
        [Route("api/SAPExtractor/GetObjectTypesNetwork")]
        public async Task<Neo4JGraphDto> GetObjectTypesNetwork()
        {
            var objectTypes = await this.neo4JConnector.GetObjectTypesNetwork();

            return objectTypes;
        }

        [HttpGet]
        [Route("api/SAPExtractor/GetObjectTypesNetwork")]
        public async Task<Neo4JGraphDto> GetObjectTypesNetwork(string objectTypeName)
        {
            var objectTypes = await this.neo4JConnector.GetObjectTypesNetwork(objectTypeName);

            return objectTypes;
        }
        

        [HttpGet]
        [Route("api/SAPExtractor/GetRelevantTablesForObjectType")]
        public async Task<RelevantTableGraphDto> GetRelevantTablesForObjectType(string objectType, int entrycount)
        {
            var relevantNodesNetwork = await this.neo4JConnector.GetRelevantTablesForObjectType(objectType, entrycount);

            return relevantNodesNetwork;
        }

        [HttpPost]
        [Route("api/SAPExtractor/GetFieldValuesForField")]
        public async Task<List<string>> GetFieldValuesForField([FromBody] ExtractorPrimKeyDto extractorPrimKeyDto)
        {
            List<string> returnValues = new List<string>();

            var tableForField = await this.neo4JConnector.GetMainTableForKey(extractorPrimKeyDto.fieldDto, extractorPrimKeyDto.tables);

            if (tableForField != null)
            {
                returnValues = this.oracleConnector.GetPrimaryKeyValues(extractorPrimKeyDto.fieldDto, tableForField.Name);
            }
            else
            {
                returnValues = this.oracleConnector.GetPrimaryKeyValues(extractorPrimKeyDto.fieldDto, extractorPrimKeyDto.tables);
            }


            return returnValues;
        }


        #endregion

        #region SAP Database Extraction


        [HttpGet]
        [Route("api/SAPExtractor/CheckOracleDBConnection")]
        public bool CheckOracleDBConnection()
        {
            OracleConnection con = oracleConnector.ConnectToOracle();

            Console.WriteLine("Connection established (" + con.ServerVersion + ")");

            return true;
        }

        [HttpPost]
        [Route("api/SAPExtractor/SaveOracleDBSchemeToNeo4J")]
        public async void SaveOracleDBSchemeToNeo4J()
        {
            await this.ImportSAPData();

            await this.CreateExtendedSAPRealations();
        }

        [HttpPost]
        [Route("api/SAPExtractor/SavePrimaryKeysToNeo4J")]
        public async void SavePrimaryKeysToNeo4J()
        {
            await this.ImportSAPData();
        }


        /// <summary>
        /// Creates the extended SAP Relations based on Pattern searches in the Graphdatabase.
        /// </summary>
        [HttpPost]
        [Route("api/SAPExtractor/CreateExtendedSAPRealations")]
        public async Task CreateExtendedSAPRealations()
        {
            await this.neo4JConnector.CreateExtendedSAPGraph();
        }

        [HttpPost]
        [Route("api/SAPExtractor/SaveProcessDataToNeo4J")]
        public async void SaveProcessDataToNeo4J()
        {
            
        }

        [HttpPost]
        [Route("api/SAPExtractor/ImportTCodesToNeo4J")]
        public async void ImportTCodesToNeo4J()
        {
            List<Neo4JNodeDto> tables = await neo4JConnector.GetAllTables();
            List<Neo4JNodeDto> tCodes = this.oracleConnector.GetTCodeNodes();

            List<SAPTableRelationDto> tCodeRelations = this.oracleConnector.GetTCodeTableRelations();

            tCodes = await neo4JConnector.ImportNodesBulk(tCodes);
            await neo4JConnector.ImportSAPRelations(tCodeRelations, tCodes, tables);
        }

        [HttpPost]
        [Route("api/SAPExtractor/ImportTCodesToNeo4J2")]
        public async void ImportTCodes2ToNeo4J()
        {
            List<Neo4JNodeDto> tables = await neo4JConnector.GetAllTables();
            List<Neo4JNodeDto> tCodes = await neo4JConnector.GetAllTCodes();

            List<SAPTableRelationDto> tCodeRelations = this.oracleConnector.GetTCodeTableRelations();

            await neo4JConnector.ImportSAPRelations(tCodeRelations, tCodes, tables);
        }

        [HttpGet]
        [Route("api/SAPExtractor/PerformExtractionTest")]
        public async Task PerformExtractionTest(SAPTableNode sapTableNodeDtos)
        {
            var client = new RestClient(this.pythonAPIUrl);
            var request = new RestRequest("newExtractorPerformExtractionNew");


            List<Neo4JNodeDto> tables = await neo4JConnector.GetAllTables();
            List<Neo4JNodeDto> tablesSmall = new List<Neo4JNodeDto>();


            Neo4JNodeDto ekkoTable = tables.FirstOrDefault(x => x.Name.Equals("EKKO"));
            Neo4JNodeDto ekpoTable = tables.FirstOrDefault(x => x.Name.Equals("EKPO"));
            Neo4JNodeDto ebanTable = tables.FirstOrDefault(x => x.Name.Equals("EBAN"));

            tablesSmall.Add(ekkoTable);
            tablesSmall.Add(ekpoTable);
            tablesSmall.Add(ebanTable);

            Db_con_args dbConArgs = new Db_con_args();
            dbConArgs.hostname = this.oracle_hostname;
            dbConArgs.password = this.password;
            dbConArgs.username = this.username;
            dbConArgs.port = this.oracle_port;
            dbConArgs.sid = this.oracle_sid;

            request.AddParameter("db_type", "oracle");
            request.AddParameter("db_con_args", JsonConvert.SerializeObject(dbConArgs));
            request.AddParameter("tabs", JsonConvert.SerializeObject(tablesSmall));
            request.AddParameter("mandt", 800);
            
            
            var response = client.Get(request);
            var content = response.Content; // Raw content as string
        }


        [HttpPost]
        [Route("api/SAPExtractor/PerformExtraction")]
        public async Task PerformExtraction([FromBody] ExtractionInputDto extractionInputDto)
        {
            var client = new RestClient(this.pythonAPIUrl);
            var request = new RestRequest("newExtractorPerformExtractionNew");



            List<Neo4JNodeDto> tables = await neo4JConnector.GetAllTables();
            List<Neo4JNodeDto> tablesSmall = new List<Neo4JNodeDto>();


            foreach (NodeTableDto nodeTableDto in extractionInputDto.Tables)
            {
                Neo4JNodeDto foundTable = tables.FirstOrDefault(x => x.Name.Equals(nodeTableDto.Name));

                tablesSmall.Add(foundTable);
            }

            Db_con_args dbConArgs = new Db_con_args();
            dbConArgs.hostname = this.oracle_hostname;
            dbConArgs.password = this.password;
            dbConArgs.username = this.username;
            dbConArgs.port = this.oracle_port;
            dbConArgs.sid = this.oracle_sid;

            request.AddParameter("db_type", "oracle");
            request.AddParameter("db_con_args", JsonConvert.SerializeObject(dbConArgs));
            request.AddParameter("tabs", JsonConvert.SerializeObject(tablesSmall));
            request.AddParameter("mandt", 800);
            

            var response = client.Get(request);
            var content = response.Content; 
        }

        #region Helper Methods

        /// <summary>
        /// Imports the data from SAP to the Graph Database
        /// </summary>
        /// <returns></returns>
        private async Task ImportSAPData()
        {
            SAPNeo4JCompleteGraphDto sapNeo4JCompleteGraphDto = this.oracleConnector.GetSAPGraph();
            sapNeo4JCompleteGraphDto.TableNodes = await neo4JConnector.ImportNodesBulk(sapNeo4JCompleteGraphDto.TableNodes);
            sapNeo4JCompleteGraphDto.TableKeyFieldNodes = await neo4JConnector.ImportNodesBulk(sapNeo4JCompleteGraphDto.TableKeyFieldNodes);
            sapNeo4JCompleteGraphDto.TableDateFieldNodes = await neo4JConnector.ImportNodesBulk(sapNeo4JCompleteGraphDto.TableDateFieldNodes);
            sapNeo4JCompleteGraphDto.TableResourceFieldNodes = await neo4JConnector.ImportNodesBulk(sapNeo4JCompleteGraphDto.TableResourceFieldNodes);
            sapNeo4JCompleteGraphDto.TableTimeFieldNodes = await neo4JConnector.ImportNodesBulk(sapNeo4JCompleteGraphDto.TableTimeFieldNodes);
            // sapNeo4JCompleteGraphDto.TableFieldNodes = await neo4JConnector.ImportNodesBulk(sapNeo4JCompleteGraphDto.TableFieldNodes);

            sapNeo4JCompleteGraphDto.ApplicationClassNodes = await neo4JConnector.ImportNodesBulk(sapNeo4JCompleteGraphDto.ApplicationClassNodes);
            sapNeo4JCompleteGraphDto.ObjecttypeNode = await neo4JConnector.ImportNodesBulk(sapNeo4JCompleteGraphDto.ObjecttypeNode);

            SAPNeo4JGraphDto domainGraph = this.oracleConnector.GetSAPDomainGraph(sapNeo4JCompleteGraphDto.TableKeyFieldNodes);
            domainGraph.Nodes = await neo4JConnector.ImportNodesBulk(domainGraph.Nodes);


            await neo4JConnector.ImportSAPRelations(sapNeo4JCompleteGraphDto.TableEdges, sapNeo4JCompleteGraphDto.TableNodes, sapNeo4JCompleteGraphDto.TableNodes);
            await neo4JConnector.ImportSAPRelations(sapNeo4JCompleteGraphDto.TableKeyFieldEdges, sapNeo4JCompleteGraphDto.TableNodes, sapNeo4JCompleteGraphDto.TableKeyFieldNodes);
            await neo4JConnector.ImportSAPRelations(sapNeo4JCompleteGraphDto.TableDateFieldEdges, sapNeo4JCompleteGraphDto.TableNodes, sapNeo4JCompleteGraphDto.TableDateFieldNodes);
            await neo4JConnector.ImportSAPRelations(sapNeo4JCompleteGraphDto.TableTimeFieldEdges, sapNeo4JCompleteGraphDto.TableNodes, sapNeo4JCompleteGraphDto.TableTimeFieldNodes);
            await neo4JConnector.ImportSAPRelations(sapNeo4JCompleteGraphDto.TableResourceFieldEdges, sapNeo4JCompleteGraphDto.TableNodes, sapNeo4JCompleteGraphDto.TableResourceFieldNodes);
            // await neo4JConnector.ImportSAPRelations(sapNeo4JCompleteGraphDto.TableFieldEdges, sapNeo4JCompleteGraphDto.TableNodes, sapNeo4JCompleteGraphDto.TableFieldNodes);
            
            await neo4JConnector.ImportSAPRelations(sapNeo4JCompleteGraphDto.ApplicationClassEdges, sapNeo4JCompleteGraphDto.TableNodes, sapNeo4JCompleteGraphDto.ApplicationClassNodes);
            await neo4JConnector.ImportSAPRelations(sapNeo4JCompleteGraphDto.ObjecttypeEdges, sapNeo4JCompleteGraphDto.ObjecttypeNode, sapNeo4JCompleteGraphDto.TableNodes);

            await neo4JConnector.ImportSAPRelations(domainGraph.Edges,
                sapNeo4JCompleteGraphDto.TableNodes.Concat(sapNeo4JCompleteGraphDto.TableKeyFieldNodes).ToList(),
                domainGraph.Nodes);
        }


        #endregion

        #endregion
    }
}