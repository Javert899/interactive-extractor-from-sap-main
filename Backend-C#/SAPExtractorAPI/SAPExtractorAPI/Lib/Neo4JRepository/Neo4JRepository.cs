using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using log4net;
using Neo4jClient;
using Newtonsoft.Json;
using SAPExtractorAPI.Lib.Mapper;
using SAPExtractorAPI.Models.Helper;
using SAPExtractorAPI.Models.Neo4J;

namespace SAPExtractorAPI.Lib.Neo4JRepository
{
    public class Neo4JRepository : Neo4JBaseRepository.Neo4JBaseRepository, INeo4JRepository
    {

        protected readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        public Neo4JRepository(string neo4jurl, string neo4juser, string neo4jpass) : base(neo4jurl, neo4juser, neo4jpass)
        {
            // sDefaultRelationType = "Standard";
        }
    }
}
