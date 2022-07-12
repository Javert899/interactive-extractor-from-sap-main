using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using log4net;
using Microsoft.VisualBasic.Logging;
using SAPExtractorAPI.Models.Neo4J;
using Neo4jClient;
using Neo4jClient.Cypher;
using SAPExtractorAPI.Models.Helper;

namespace SAPExtractorAPI.Lib.Neo4JBaseRepository
{
    public abstract partial class Neo4JBaseRepository: INeo4JBaseRepository
    {
        public GraphClient Client;
        protected readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected static object _lock = new object();
        protected string DefaultRelationType = "CONNECT_WITH";

        protected const string NodeTempName = "component";

        protected const string RelationTempName = "relation";

        protected const string ResultComponentName = "result";

        protected static class MatchingExpression
        {
            public const string StartsWith = "STARTS WITH";
            public const string EndsWith = "ENDS WITH";
            public const string Contains = "CONTAINS";
            public const string EqualsExpr = "=";
        }

        /// <summary>
        /// Gibt die entsprechende Where expression zugehoerig zum Filter zurueck
        /// </summary>
        /// <param name="filterCondition">Filter</param>
        /// <param name="nodeName">Name des Knotens innerhalb der query z.B. bei "where NODENAME.Name = 'ABC'" ist NODENAME der nodeName   </param>
        /// <param name="propName">Eigenschaft der Knotens nach dem gefiltert werden soll</param>
        /// <param name="value">Wert</param>
        /// <param name="caseSensitive"> auf true, falls nach Gross- und Kleinschreibung gefiltert werden soll</param>
        /// <returns></returns>
        protected string GetMatchingExpression(FilterCondition filterCondition, string nodeName, string propName,
            string value, bool caseSensitive = false)
        {
            string toLowerExpressStart = "";
            string toLowerExpressEnd = "";
            string not = "";

            if (!caseSensitive)
            {
                toLowerExpressStart = "toLower(";
                toLowerExpressEnd = ")";
                value = value.ToLower();
            }

            string matchingExpr;

            switch (filterCondition)
            {
                case FilterCondition.StartsWith:
                    matchingExpr = MatchingExpression.StartsWith;
                    break;
                case FilterCondition.EndsWith:
                    matchingExpr = MatchingExpression.EndsWith;
                    break;
                case FilterCondition.Contains:
                    matchingExpr = MatchingExpression.Contains;
                    break;
                case FilterCondition.Equals:
                    matchingExpr = MatchingExpression.EqualsExpr;
                    break;
                case FilterCondition.DoesNotContain:
                    matchingExpr = MatchingExpression.Contains;
                    not = "NOT ";
                    break;
                case FilterCondition.DoesNotEqual:
                    not = "NOT ";
                    matchingExpr = MatchingExpression.EqualsExpr;
                    break;
                default:
                    return "";


            }

            return string.Format("{0}{1}{2}.`{3}`{4} {5} {1}'{6}'{4}", not, toLowerExpressStart, nodeName, propName,
                toLowerExpressEnd, matchingExpr, value);
        }



        public Neo4JBaseRepository(string neo4jurl, string neo4juser, string neo4jpass)
        {
            try
            {
                this.Client = new GraphClient(new Uri(neo4jurl), neo4juser, neo4jpass);
                this.Client.ConnectAsync().Wait();
            }
            catch (Exception e)
            {
            }
        }

        public void Connect(string neo4jurl, string neo4juser, string neo4jpass)
        {
            try
            {
                this.Client = new GraphClient(new Uri(neo4jurl), neo4juser, neo4jpass);
                this.Client.ConnectAsync().Wait();
            }
            catch (Exception e)
            {
            }
        }

        #region Stuff

        /// <summary>
        /// Ueberprueft, ob der Wert gültig ist 
        /// Vermeidet SQL Injections
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected bool IsValidInputQuery(string value)
        {
            //bool valid = Regex.IsMatch(value, @"^[a-zA-Z0-9_ -]+$");
            //if (!valid)
            //{
            //    Log.WarnFormat("Der angegebene Value hat ungültige Zeichen (Es duerfen nur Buchstaben, Untenstriche und Zahlen verwendet werden): " + value);
            //}
            return value != null && !value.Contains("`");

        }

        protected bool IsValidLabelInput(string label)
        {
            return IsValidInputQuery(label) && !string.IsNullOrEmpty(label) && !label.Contains("\"");

        }

        /// <summary>
        /// Gibt das Query fuer den Knoten mit der Id zurueck
        /// VORSICHT: Es muss mit dem  Knotenparameter "n" weitergearbeitet werden
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected ICypherFluentQuery GetNodeByIdQuery(long id)
        {
            ICypherFluentQuery query = Client.Cypher.Match("(n)")
                .Where("n.Id = {idParam}").WithParam("idParam", id);

            return query;
        }

        public async Task<List<string>> GetAllNodeLabels()
        {
            return (await  Client.Cypher.Match("(n)").ReturnDistinct(n => n.Labels()).ResultsAsync).SelectMany(x => x).Distinct()
                .ToList();
        }

        public async Task<List<string>> GetAllRelationTypes()
        {
            IEnumerable<string> types = await Client.Cypher.Match("()-[r]-()").ReturnDistinct(r => r.Type()).ResultsAsync;


            return types.ToList();
        }
        /// <summary>
        /// Fragt ab ob die Verbindung zu Neo4J steht
        /// </summary>
        /// <returns></returns>
        public bool IsConnected()
        {
            return Client != null && Client.IsConnected;
        }


        public async void ChangeLabelName(string oldName, string newName)
        {

            if (string.IsNullOrEmpty(oldName) || string.IsNullOrEmpty(newName))
            {
                Log.WarnFormat(string.Format(
                    "alter oder neuer Labelname ist leer: Die Umbennenung der Labels geht nur, wenn der alte und neue Name existieren (alter: {0}, neuer: {1})",
                    oldName ?? "", newName ?? ""));

                return;
            }

            await Client.Cypher.Match(string.Format("(n:`{0}`)", oldName)).Remove(string.Format("n:`{0}`", oldName)).Set(string.Format("n:`{0}`", newName))
                .ExecuteWithoutResultsAsync();

        }


        /// <summary>
        /// Executes a given query.
        /// </summary>
        /// <param name="query">The query to execute.</param>
        /// <returns></returns>
        public async Task ExecuteQueryWithoutResult(ICypherFluentQuery query)
        {
            try
            {
                await query.ExecuteWithoutResultsAsync();
            }
            catch (Exception ex)
            {

            }
        }

        #endregion

    }

}
