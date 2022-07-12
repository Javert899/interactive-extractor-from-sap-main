using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using SAPExtractorAPI.Lib;

namespace SAPExtractorAPI.Controllers
{
    public class Neo4JController : ApiController
    {
        string neo4jUrl = ConfigurationManager.AppSettings["neodburi"];
        string neo4jpass = ConfigurationManager.AppSettings["neodbpass"];
        string neo4juser = ConfigurationManager.AppSettings["neodbuser"];

        private OracleConnector oracleConnector = new OracleConnector();

        private Neo4JConnector neo4JConnector = new Neo4JConnector();

        private Lib.Neo4JBaseRepository.Neo4JBaseRepository neo4Repository;

        public Neo4JController()
        {
            neo4Repository = new Lib.Neo4JRepository.Neo4JRepository(neo4jUrl, neo4juser, neo4jpass);
        }

        [HttpGet]
        [Route("api/NEO4J/GetServerVersion")]
        public string GetServerVersion()
        {
            return "1.2";
        }

        [HttpGet]
        [Route("api/NEO4J/GetNodeCount")]
        public async Task<int> GetNodeCount(string labelFilter)
        {
            bool connected = neo4Repository.IsConnected();
            this.neo4Repository.Connect(neo4jUrl, neo4juser, neo4jpass);

            if (!connected)
            {
                neo4Repository = new Lib.Neo4JRepository.Neo4JRepository(neo4jUrl, neo4juser, neo4jpass);
            }


            return 0;
        }

        
    }
}