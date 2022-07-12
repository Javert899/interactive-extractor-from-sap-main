using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using Microsoft.Ajax.Utilities;
using Oracle.ManagedDataAccess.Client;
using SAPExtractorAPI.Models.Neo4J;
using SAPExtractorAPI.Models.Neo4J.NodeTypes;
using SAPExtractorAPI.Models.SAP;

namespace SAPExtractorAPI.Lib
{
    public class OracleConnector
    {
        private string table_prefix = ConfigurationManager.AppSettings["table_prefix"];
        private OracleConnection con;

        /// <summary>
        /// Initiate the connectio to the Oracle Database.
        /// </summary>
        /// <returns></returns>
        public OracleConnection ConnectToOracle()
        {
            // Load Connection Parameters
            string username = ConfigurationManager.AppSettings["oracle_username"];
            string password = ConfigurationManager.AppSettings["oracle_password"];
            string oracle_hostname = ConfigurationManager.AppSettings["oracle_hostname"];
            string oracle_port = ConfigurationManager.AppSettings["oracle_port"];
            string oracle_sid = ConfigurationManager.AppSettings["oracle_sid"];

            // Build datasource string
            string datasource = oracle_hostname + ":" + oracle_port + "/" + oracle_sid;

            // create connection
            con = this.ConnectToOracle(username, password, datasource);

            return con;
        }


        /// <summary>
        /// Initiate the connectio to the Oracle Database with given credentials.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="datasource"></param>
        /// <returns></returns>
        public OracleConnection ConnectToOracle(string username, string password, string datasource)
        {
            // create connection
            con = new OracleConnection();

            // create connection string using builder
            OracleConnectionStringBuilder ocsb = new OracleConnectionStringBuilder();
            ocsb.Password = password;
            ocsb.UserID = username;

            // ocsb.DataSource = "database.url:port/databasename";
            ocsb.DataSource = datasource;

            // connect
            con.ConnectionString = ocsb.ConnectionString;
            con.Open();

            return con;
        }


        /// <summary>
        /// Optimated Version of the SAP Graph DTO. Directly Transform Data in Neo4J Data Types.
        /// </summary>
        /// <returns></returns>
        public SAPNeo4JCompleteGraphDto GetSAPGraph()
        {
            SAPNeo4JCompleteGraphDto neo4JCompleteGraphDto = new SAPNeo4JCompleteGraphDto();

            // Get All Tables and their relations between them. 
            neo4JCompleteGraphDto.TableNodes = this.GetSAPTableNodes();
            neo4JCompleteGraphDto.TableEdges = this.GetCheckRefTableRelations();

            // Get the Tables Primary Keys and the relations to the tables.
            neo4JCompleteGraphDto.TableKeyFieldNodes = this.GetSAPKeyFieldNodes();
            neo4JCompleteGraphDto.TableKeyFieldEdges = this.GetKeyFieldRelations();

            // Get the tables Date relevant Properties and Fields. 
            neo4JCompleteGraphDto.TableDateFieldNodes = this.GetSAPDateFieldNodes();
            neo4JCompleteGraphDto.TableDateFieldEdges = this.GetSAPDateFieldRelations();

            // Get the tables Time relevant Properties and Fields. 
            neo4JCompleteGraphDto.TableTimeFieldNodes = this.GetSAPTimeFieldNodes();
            neo4JCompleteGraphDto.TableTimeFieldEdges = this.GetSAPTimeFieldRelations();

            // Get the tables Resource/Username relevant properties and Fields
            neo4JCompleteGraphDto.TableResourceFieldNodes = this.GetSAPResourceFieldNodes();
            neo4JCompleteGraphDto.TableResourceFieldEdges = this.GetSAPResourceFieldRelations();

            // Get all other Table Fields and connect them to the tables.
            // neo4JCompleteGraphDto.TableFieldNodes = this.GetSAPFieldNodes();
            // neo4JCompleteGraphDto.TableFieldEdges = this.GetSAPFieldRelations();

            // Get the Application Classes and their Hierarchy and relations to different tables.
            SAPNeo4JGraphDto applClassData = this.GetApplicationClassData();
            neo4JCompleteGraphDto.ApplicationClassNodes = applClassData.Nodes;
            neo4JCompleteGraphDto.ApplicationClassEdges = applClassData.Edges;

            // Get the object types/entities from the SAP System
            SAPNeo4JGraphDto objTypeData = this.GetObjectTypesData();
            neo4JCompleteGraphDto.ObjecttypeNode = objTypeData.Nodes;
            neo4JCompleteGraphDto.ObjecttypeEdges = objTypeData.Edges;

            return neo4JCompleteGraphDto;
        }


        public SAPNeo4JCompleteGraphDto GetSAPPrimaryKeyGraph()
        {
            SAPNeo4JCompleteGraphDto neo4JCompleteGraphDto = new SAPNeo4JCompleteGraphDto();

            // Get All Tables and their relations between them. 
            neo4JCompleteGraphDto.TableNodes = this.GetSAPTableNodes();
            neo4JCompleteGraphDto.TableEdges = this.GetCheckRefTableRelations();

            // Get the Tables Primary Keys and the relations to the tables.
            neo4JCompleteGraphDto.TableKeyFieldNodes = this.GetSAPKeyFieldNodes();
            neo4JCompleteGraphDto.TableKeyFieldEdges = this.GetKeyFieldRelations();

            return neo4JCompleteGraphDto;
        }

        /// <summary>
        /// Returns all primary key values for given table and primary keys.
        /// </summary>
        /// <param name="fieldDto"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        internal List<string> GetPrimaryKeyValues(NodeKeyDto fieldDto, string tableName)
        {
            List<string> values = new List<string>();

            OracleConnection con = this.ConnectToOracle();
            OracleCommand getTablesCommand = new OracleCommand();

            // SQL Command with Row Count
            getTablesCommand.CommandText =
                "SELECT DISTINCT " + fieldDto.Alias + " from " + this.table_prefix + tableName + " GROUP BY " +
                fieldDto.Alias;

            getTablesCommand.Connection = con;
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }

            OracleDataReader dr = getTablesCommand.ExecuteReader();
            if (dr.HasRows)
            {
                // Füge nun alle gefundenen Tabellen der Liste hinzu.
                while (dr.Read())
                {
                    string currValue = dr[fieldDto.Alias].ToString();

                    values.Add(currValue);
                }
            }


            con.Close();

            return values;
        }


        /// <summary>
        /// Returns all primary key values for given list of tables and primary keys.
        /// </summary>
        /// <param name="fieldDto"></param>
        /// <param name="tables"></param>
        /// <returns></returns>
        internal List<string> GetPrimaryKeyValues(NodeKeyDto fieldDto, List<SAPTableNode> tables)
        {
            List<string> values = new List<string>();

            if (fieldDto != null && tables != null)
            {
                if (!fieldDto.Name.IsNullOrWhiteSpace() && !fieldDto.Alias.IsNullOrWhiteSpace())
                {
                    OracleConnection con = this.ConnectToOracle();
                    OracleCommand getTablesCommand = new OracleCommand();

                    string sqlQuery = "SELECT DISTINCT " + fieldDto.Alias + " from (";

                    foreach (SAPTableNode sapTableNode in tables)
                    {
                        sqlQuery += "(SELECT " + fieldDto.Alias + " from " + this.table_prefix +
                                    sapTableNode.TableNode.Name +
                                    " GROUP BY " + fieldDto.Alias + ")";

                        if (sapTableNode != tables.LastOrDefault())
                        {
                            sqlQuery += " UNION";
                        }
                    }


                    sqlQuery += ")";


                    // SQL Command with Row Count
                    getTablesCommand.CommandText = sqlQuery;

                    getTablesCommand.Connection = con;
                    if (con.State != ConnectionState.Open)
                    {
                        con.Open();
                    }

                    OracleDataReader dr = getTablesCommand.ExecuteReader();
                    if (dr.HasRows)
                    {
                        // Füge nun alle gefundenen Tabellen der Liste hinzu.
                        while (dr.Read())
                        {
                            string currValue = dr[fieldDto.Alias].ToString();

                            values.Add(currValue);
                        }
                    }


                    con.Close();
                }
            }

            return values;
        }


        public List<Neo4JNodeDto> GetSAPTableNodes()
        {
            List<Neo4JNodeDto> sapTableNodes = new List<Neo4JNodeDto>();

            OracleConnection con = this.ConnectToOracle();
            OracleCommand getTablesCommand = new OracleCommand();

            // Die Tabelle DD02V enthält zusätzlich noch die Klartextbesschreibungen.
            // Hierbei wählen wir nur die englischen Beschreibungen
            // getTablesCommand.CommandText = "Select TABNAME, DDTEXT, TABCLASS, APPLCLASS, CONTFLAG from SAPSR3.DD02V where tabclass='TRANSP' and contflag = 'A' and ddlanguage = 'E'";


            // SQL Command with Row Count
            getTablesCommand.CommandText =
                "Select A.TABNAME, A.DDTEXT, A.TABCLASS, A.APPLCLASS, A.CONTFLAG, B.NROWS from " + this.table_prefix +
                "DD02V A join SAPSR3.DBSTATTORA B on A.tabname = B.tname where tabclass='TRANSP' and ddlanguage = 'E' and contflag = 'A' and NROWS > 0";

            getTablesCommand.Connection = con;
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }

            OracleDataReader dr = getTablesCommand.ExecuteReader();
            if (dr.HasRows)
            {
                // Füge nun alle gefundenen Tabellen der Liste hinzu.
                while (dr.Read())
                {
                    var tableName = dr["TABNAME"].ToString();

                    Neo4JNodeDto neo4JNodeDto = new Neo4JNodeDto();
                    neo4JNodeDto.Label = "SAP:TABLE:CLUSTERING";
                    neo4JNodeDto.Name = tableName;
                    neo4JNodeDto.Alias = tableName;

                    neo4JNodeDto.AddProperty("APPLCLASS", dr["APPLCLASS"].ToString());
                    neo4JNodeDto.AddProperty("Description", dr["DDTEXT"].ToString());
                    neo4JNodeDto.AddProperty("TABCLASS", dr["TABCLASS"].ToString());
                    neo4JNodeDto.AddProperty("CONTFLAG", dr["CONTFLAG"].ToString());
                    neo4JNodeDto.AddProperty("ENTRYCOUNT", int.Parse(dr["NROWS"].ToString()));


                    sapTableNodes.Add(neo4JNodeDto);
                }
            }

            return sapTableNodes;
        }

        public List<Neo4JNodeDto> GetSAPKeyFieldNodes()
        {
            List<Neo4JNodeDto> sapKeyFieldNodes = new List<Neo4JNodeDto>();

            OracleConnection con = this.ConnectToOracle();
            OracleCommand getTablesCommand = new OracleCommand();

            // Die Tabelle DD02V enthält zusätzlich noch die Klartextbesschreibungen.
            // Hierbei wählen wir nur die englischen Beschreibungen

            /*
            ALL TABLE RELATIONS
            getTablesCommand.CommandText =
                "Select DISTINCT fieldname, rollname from SAPSR3.dd03vv where tabname IN(Select Tabname from SAPSR3.DD02V A join SAPSR3.DBSTATTORA B on A.tabname = B.tname where tabclass='TRANSP' and ddlanguage = 'E' and contflag = 'A' and NROWS > 0) and tabclass='TRANSP' and ((checktable <> ' ' and checktable <> '*') or (reftable) <>' ' or keyflag = 'X')";
            */
            // TABLE RELATIONS ONLY FOR NOT EMPTY TABLES
            // getTablesCommand.CommandText = "Select DISTINCT fieldname, rollname, domname from SAPSR3.dd03vv where tabname IN(Select Tabname from SAPSR3.DD02V A join SAPSR3.DBSTATTORA B on A.tabname = B.tname where tabclass='TRANSP' and ddlanguage = 'E' and contflag = 'A' and NROWS > 0) and tabclass='TRANSP' and ((checktable <> ' ' and checktable <> '*') or (reftable) <>' ' or keyflag = 'X')";

            // Hole nur die Felder die Key sind.
            getTablesCommand.CommandText =
                "Select DISTINCT t1.fieldname, t1.rollname, t1.domname, t1.DATATYPE, t1.POSITION, t2.ddtext from " +
                this.table_prefix +
                "dd03vv t1 join Sapsr3.DD04V t2 on t1.rollname = t2.rollname and t1.domname = t2.domname where tabname IN(Select Tabname from SAPSR3.DD02V A join SAPSR3.DBSTATTORA B on A.tabname = B.tname where tabclass='TRANSP' and ddlanguage = 'E' and contflag = 'A' and NROWS > 0) and t2.ddlanguage = 'E' and t1.tabclass='TRANSP' and (t1.keyflag = 'X')";
            getTablesCommand.Connection = con;
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }

            OracleDataReader dr = getTablesCommand.ExecuteReader();
            if (dr.HasRows)
            {
                // Füge nun alle gefundenen Tabellen der Liste hinzu.
                while (dr.Read())
                {
                    // Current Field
                    string fieldname = dr["FIELDNAME"].ToString();
                    if (fieldname.Equals("BELNR"))
                    {
                        var a = 1;
                    }

                    // MANDT ist in jeder Tabelle vorhanden daher schlecht für das Clustern. 
                    if (!fieldname.Contains("MANDT") && !fieldname.Contains(".INCLUDE"))
                    {
                        string rollname = dr["ROLLNAME"].ToString();
                        string domname = dr["domname"].ToString();
                        string datatype = dr["DATATYPE"].ToString();
                        int position = int.Parse(dr["POSITION"].ToString());

                        string description = dr["ddtext"].ToString();

                        // Excluded Fieldnames: DB_KEY, GUID
                        /*
                        string[] exludedDoms = new[]
                        {
                            "MANDT",
                            "WERKS",
                            "SYSUUID",
                            "OBJPS",
                            "SUBTY",
                            "SPRPS",
                            "PERSNO",
                            "NUM03"
                        };
                        */
                        //TODO Evtl über Typ rausfiltern?
                        //Sortiere nicht relevante Datentypen raus
                        if (
                            true) //!datatype.Equals("CLNT") && !datatype.Equals("LANG") && !datatype.Equals("DATS") && !exludedDoms.Contains(domname))
                        {
                            string keyFieldName = "";
                            if (String.IsNullOrWhiteSpace(rollname))
                            {
                                keyFieldName = fieldname;
                            }
                            else
                            {
                                keyFieldName = fieldname + ": " + rollname;
                                // keyFieldName = fieldname + ": " + domname;
                            }

                            Neo4JNodeDto keyFieldNeo4JNodeDto = new Neo4JNodeDto();
                            keyFieldNeo4JNodeDto.Label = "SAP:KEY:CLUSTERING";


                            // Unify name with the Role Name
                            keyFieldNeo4JNodeDto.Name = keyFieldName;
                            keyFieldNeo4JNodeDto.Alias = fieldname;

                            keyFieldNeo4JNodeDto.AddProperty("FIELDNAME", fieldname);
                            keyFieldNeo4JNodeDto.AddProperty("ROLLNAME", rollname);
                            keyFieldNeo4JNodeDto.AddProperty("DOMNAME", domname);
                            keyFieldNeo4JNodeDto.AddProperty("DATATYPE", datatype);
                            keyFieldNeo4JNodeDto.AddProperty("Description", description);
                            keyFieldNeo4JNodeDto.AddProperty("Position", position);


                            sapKeyFieldNodes.Add(keyFieldNeo4JNodeDto);
                        }
                    }
                }
            }

            return sapKeyFieldNodes;
        }

        public List<SAPTableRelationDto> GetCheckRefTableRelations()
        {
            List<SAPTableRelationDto> checkTableRelations = new List<SAPTableRelationDto>();

            OracleCommand cmd = new OracleCommand();
            // Go through all Ref Tables and Check Tables
            // Optimierte SQL Query

            //TODO ONLY NOT EMPTY TABLES?
            cmd.CommandText =
                "Select TABNAME,CHECKTABLE,REFTABLE,REFFIELD,FIELDNAME,KEYFLAG,DATATYPE,DOMNAME,ROLLNAME from " +
                this.table_prefix +
                "dd03vv where tabname IN(Select Tabname from SAPSR3.DD02V A join SAPSR3.DBSTATTORA B on A.tabname = B.tname where tabclass='TRANSP' and ddlanguage = 'E' and contflag = 'A' and NROWS > 0) and tabclass='TRANSP' and ((checktable <> ' ' and checktable <> '*') or (reftable) <>' ')";

            cmd.Connection = con;
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }

            // Evtl erst alle Rows durchgehen und abspeichern und dann alle Tabellen aus dem ersten Abruf durchgehen? 
            OracleDataReader dr = cmd.ExecuteReader();
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    // Tablename
                    string tabname = dr["TABNAME"].ToString();
                    // Checktable
                    var checkTableName = dr["CHECKTABLE"].ToString();
                    // Ref Infos
                    var refTableName = dr["REFTABLE"].ToString();
                    var refFieldName = dr["REFFIELD"].ToString();
                    // Current Field
                    string fieldname = dr["FIELDNAME"].ToString();
                    // Field is Key ?
                    bool isKeyField = dr["KEYFLAG"].ToString().Equals("X");

                    if (!String.IsNullOrEmpty(checkTableName) &&
                        !String.IsNullOrWhiteSpace(checkTableName)) //existingRelation == null)
                    {
                        if (!checkTableName.Equals("*"))
                        {
                            SAPTableRelationDto sapTableRelationDto = new SAPTableRelationDto();

                            sapTableRelationDto.Node1Name = tabname;
                            sapTableRelationDto.Node2Name = checkTableName;
                            sapTableRelationDto.RelationshipType = "IS_CHECKED_BY";

                            SAPRelationProperty sapRelationProperty = new SAPRelationProperty
                            {
                                PropertyName = "Field",
                                PropertyValue = fieldname
                            };


                            sapTableRelationDto.RelationProperties.Add(sapRelationProperty);


                            checkTableRelations.Add(sapTableRelationDto);
                        }
                    }

                    if (!String.IsNullOrEmpty(refTableName) && !String.IsNullOrWhiteSpace(refTableName))
                    {
                        /*
                        SAPTableRelationDto existingRelation = neo4JGraphDto.TableEdges.FirstOrDefault(x =>
                            x.Node1Name.Equals(tableNodeDto.Name) &&
                            x.Node2Name.Equals(refTableName));
                        */
                        if (true) //existingRelation == null)
                        {
                            SAPTableRelationDto sapTableRelationDto = new SAPTableRelationDto();

                            sapTableRelationDto.Node1Name = tabname;
                            sapTableRelationDto.Node2Name = refTableName;
                            sapTableRelationDto.RelationshipType = "IS_REFERRING_TO";

                            // Stores on which Field the Reference is pointing
                            SAPRelationProperty sapRelationProperty = new SAPRelationProperty
                            {
                                PropertyName = "ReferencedField",
                                PropertyValue = refFieldName
                            };
                            sapTableRelationDto.RelationProperties.Add(sapRelationProperty);

                            // Store which Field uses this Reference
                            sapRelationProperty = new SAPRelationProperty
                            {
                                PropertyName = "Field",
                                PropertyValue = fieldname
                            };
                            sapTableRelationDto.RelationProperties.Add(sapRelationProperty);


                            checkTableRelations.Add(sapTableRelationDto);
                        }
                    }
                }
            }

            return checkTableRelations;
        }

        public List<SAPTableRelationDto> GetKeyFieldRelations()
        {
            List<SAPTableRelationDto> keyFieldRelations = new List<SAPTableRelationDto>();

            OracleCommand cmd = new OracleCommand();
            // Go through all Ref Tables and Check Tables
            // Optimierte SQL Query

            //TODO: Only not empty Tables?
            cmd.CommandText =
                "Select TABNAME,CHECKTABLE,REFTABLE,REFFIELD,FIELDNAME,KEYFLAG,DATATYPE,DOMNAME,ROLLNAME from " +
                this.table_prefix +
                "dd03vv where tabname IN(Select Tabname from SAPSR3.DD02V A join SAPSR3.DBSTATTORA B on A.tabname = B.tname where tabclass='TRANSP' and ddlanguage = 'E' and contflag = 'A' and NROWS > 0) and tabclass='TRANSP' and (keyflag = 'X')";

            cmd.Connection = con;
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }

            // Evtl erst alle Rows durchgehen und abspeichern und dann alle Tabellen aus dem ersten Abruf durchgehen? 
            OracleDataReader dr = cmd.ExecuteReader();
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    // Tablename
                    string tabname = dr["TABNAME"].ToString();
                    if (tabname.Equals("RSEG") || tabname.Equals("RBKP"))
                    {
                        var a = 1;
                    }

                    // Checktable
                    var checkTableName = dr["CHECKTABLE"].ToString();
                    // Ref Infos
                    var refTableName = dr["REFTABLE"].ToString();
                    var refFieldName = dr["REFFIELD"].ToString();
                    // Current Field
                    string fieldname = dr["FIELDNAME"].ToString();
                    // Field is Key ?
                    bool isKeyField = dr["KEYFLAG"].ToString().Equals("X");

                    string datatype = dr["DATATYPE"].ToString();
                    string domname = dr["DOMNAME"].ToString();
                    string rollname = dr["ROLLNAME"].ToString();

                    string keyFieldName = "";
                    if (String.IsNullOrWhiteSpace(rollname))
                    {
                        keyFieldName = fieldname;
                    }
                    else
                    {
                        keyFieldName = fieldname + ": " + rollname;
                        // keyFieldName = fieldname + ": " + domname;
                    }

                    // Add the relation between table and Key Field
                    SAPTableRelationDto sapTableRelation = new SAPTableRelationDto
                    {
                        RelationshipType = "IS_DEFINED_BY_KEY",
                        Node1Name = tabname,
                        Node2Name = keyFieldName
                    };

                    keyFieldRelations.Add(sapTableRelation);
                }
            }

            return keyFieldRelations;
        }

        #region Table Fields and Propertys

        #region Date Time and Resources

        /// <summary>
        /// Retruns the Fields with Data Type Date
        /// </summary>
        /// <returns></returns>
        public List<Neo4JNodeDto> GetSAPDateFieldNodes()
        {
            List<Neo4JNodeDto> fieldNodes = new List<Neo4JNodeDto>();

            OracleConnection con = this.ConnectToOracle();
            OracleCommand getTablesCommand = new OracleCommand();

            getTablesCommand.CommandText =
                "Select DISTINCT t1.tabname, t1.fieldname, t1.rollname, t1.domname, t1.DATATYPE, t1.POSITION, t2.ddtext from SAPSR3.dd03vv t1 join Sapsr3.DD04V t2 on t1.rollname = t2.rollname and t1.domname = t2.domname where tabname IN(Select Tabname from SAPSR3.DD02V A join SAPSR3.DBSTATTORA B on A.tabname = B.tname where tabclass='TRANSP' and ddlanguage = 'E' and contflag = 'A' and NROWS > 0) and t2.ddlanguage = 'E' and t1.tabclass='TRANSP' and t1.datatype = 'DATS' and (t1.keyflag != 'X')";
            getTablesCommand.Connection = con;
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }

            OracleDataReader dr = getTablesCommand.ExecuteReader();
            if (dr.HasRows)
            {
                // Füge nun alle gefundenen Tabellen der Liste hinzu.
                while (dr.Read())
                {
                    // Current Field
                    string fieldname = dr["FIELDNAME"].ToString();

                    if (fieldname.Equals("AEDAT"))
                    {
                        var a = "a";
                    }

                    // MANDT ist in jeder Tabelle vorhanden daher schlecht für das Clustern. 
                    if (!fieldname.Contains("MANDT") && !fieldname.Contains(".INCLUDE"))
                    {
                        string tabname = dr["TABNAME"].ToString();
                        string rollname = dr["ROLLNAME"].ToString();

                        string domname = dr["domname"].ToString();
                        string datatype = dr["DATATYPE"].ToString();
                        int position = int.Parse(dr["POSITION"].ToString());

                        string description = dr["ddtext"].ToString();


                        if (true)
                        {
                            string keyFieldName = "";
                            if (String.IsNullOrWhiteSpace(rollname))
                            {
                                keyFieldName = fieldname;
                            }
                            else
                            {
                                keyFieldName = fieldname + ":" + rollname;
                            }

                            keyFieldName += ":" + tabname;

                            Neo4JNodeDto keyFieldNeo4JNodeDto = new Neo4JNodeDto();
                            string nodeLabel = "SAP:DATEFIELD:PROPERTYFIELD";
                            // Check if Date Field is Event Related
                            if (fieldname.Equals("AEDAT") || fieldname.Equals("ERDAT"))
                            {
                                nodeLabel += ":EVENTDATE";
                            }


                            // Set the Node Label
                            keyFieldNeo4JNodeDto.Label = nodeLabel;
                            keyFieldNeo4JNodeDto.Name = keyFieldName;

                            // Unify name with the Role Name
                            keyFieldNeo4JNodeDto.Alias = fieldname;

                            keyFieldNeo4JNodeDto.AddProperty("FIELDNAME", fieldname);
                            keyFieldNeo4JNodeDto.AddProperty("ROLLNAME", rollname);
                            keyFieldNeo4JNodeDto.AddProperty("DOMNAME", domname);
                            keyFieldNeo4JNodeDto.AddProperty("DATATYPE", datatype);
                            keyFieldNeo4JNodeDto.AddProperty("Description", description);
                            keyFieldNeo4JNodeDto.AddProperty("Position", position);

                            fieldNodes.Add(keyFieldNeo4JNodeDto);
                        }
                    }
                }
            }

            return fieldNodes;
        }

        public List<SAPTableRelationDto> GetSAPDateFieldRelations()
        {
            List<SAPTableRelationDto> dateFieldRelations = new List<SAPTableRelationDto>();

            OracleCommand cmd = new OracleCommand();
            // Go through all Ref Tables and Check Tables
            // Optimierte SQL Query

            //TODO: Only not empty Tables?
            cmd.CommandText =
                "Select TABNAME,CHECKTABLE,REFTABLE,REFFIELD,FIELDNAME,KEYFLAG,DATATYPE,DOMNAME,ROLLNAME from SAPSR3.dd03vv where tabname IN(Select Tabname from SAPSR3.DD02V A join SAPSR3.DBSTATTORA B on A.tabname = B.tname where tabclass='TRANSP' and ddlanguage = 'E' and contflag = 'A' and NROWS > 0) and tabclass='TRANSP' and datatype = 'DATS'";

            cmd.Connection = con;
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }

            // Evtl erst alle Rows durchgehen und abspeichern und dann alle Tabellen aus dem ersten Abruf durchgehen? 
            OracleDataReader dr = cmd.ExecuteReader();
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    // Tablename
                    string tabname = dr["TABNAME"].ToString();

                    if (tabname.Equals("EKKO"))
                    {
                        var c = 1;
                    }

                    // Checktable
                    var checkTableName = dr["CHECKTABLE"].ToString();
                    // Ref Infos
                    var refTableName = dr["REFTABLE"].ToString();
                    var refFieldName = dr["REFFIELD"].ToString();
                    // Current Field
                    string fieldname = dr["FIELDNAME"].ToString();
                    // Field is Key ?
                    bool isKeyField = dr["KEYFLAG"].ToString().Equals("X");

                    string datatype = dr["DATATYPE"].ToString();
                    string domname = dr["DOMNAME"].ToString();
                    string rollname = dr["ROLLNAME"].ToString();

                    string keyFieldName = "";
                    if (String.IsNullOrWhiteSpace(rollname))
                    {
                        keyFieldName = fieldname;
                    }
                    else
                    {
                        keyFieldName = fieldname + ":" + rollname;
                        // keyFieldName = fieldname + ": " + domname;
                    }

                    keyFieldName += ":" + tabname;

                    string relationshipType = "IS_DEFINED_BY_DATEFIELD";
                    // Check if Date Field is Event Related
                    if (fieldname.Equals("AEDAT") || fieldname.Equals("ERDAT"))
                    {
                        relationshipType = "IS_DEFINED_BY_EVENTDATEFIELD";
                    }

                    // Add the relation between table and Key Field
                    SAPTableRelationDto sapTableRelation = new SAPTableRelationDto
                    {
                        RelationshipType = relationshipType,
                        Node1Name = tabname,
                        Node2Name = keyFieldName
                    };

                    dateFieldRelations.Add(sapTableRelation);
                }
            }

            return dateFieldRelations;
        }

        /// <summary>
        /// Retruns the Fields with Data Type Time in SAP UZEIT
        /// </summary>
        /// <returns></returns>
        public List<Neo4JNodeDto> GetSAPTimeFieldNodes()
        {
            List<Neo4JNodeDto> fieldNodes = new List<Neo4JNodeDto>();

            OracleConnection con = this.ConnectToOracle();
            OracleCommand getTablesCommand = new OracleCommand();

            getTablesCommand.CommandText =
                "Select DISTINCT t1.Tabname, t1.fieldname, t1.rollname, t1.domname, t1.DATATYPE, t1.POSITION, t2.ddtext from SAPSR3.dd03vv t1 join Sapsr3.DD04V t2 on t1.rollname = t2.rollname and t1.domname = t2.domname where tabname IN(Select Tabname from SAPSR3.DD02V A join SAPSR3.DBSTATTORA B on A.tabname = B.tname where tabclass='TRANSP' and ddlanguage = 'E' and contflag = 'A' and NROWS > 0) and t2.ddlanguage = 'E' and t1.tabclass='TRANSP' and t1.datatype = 'TIMS' and (t1.keyflag != 'X')";
            getTablesCommand.Connection = con;
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }

            OracleDataReader dr = getTablesCommand.ExecuteReader();
            if (dr.HasRows)
            {
                // Füge nun alle gefundenen Tabellen der Liste hinzu.
                while (dr.Read())
                {
                    // Current Field
                    string fieldname = dr["FIELDNAME"].ToString();

                    if (fieldname.Equals("AEDAT"))
                    {
                        var a = "a";
                    }

                    // MANDT ist in jeder Tabelle vorhanden daher schlecht für das Clustern. 
                    if (!fieldname.Contains("MANDT") && !fieldname.Contains(".INCLUDE"))
                    {
                        string tabname = dr["TABNAME"].ToString();
                        string rollname = dr["ROLLNAME"].ToString();

                        string domname = dr["domname"].ToString();
                        string datatype = dr["DATATYPE"].ToString();
                        int position = int.Parse(dr["POSITION"].ToString());

                        string description = dr["ddtext"].ToString();


                        if (true)
                        {
                            string keyFieldName = "";
                            if (String.IsNullOrWhiteSpace(rollname))
                            {
                                keyFieldName = fieldname;
                            }
                            else
                            {
                                keyFieldName = fieldname + ":" + rollname;
                            }

                            keyFieldName += ":" + tabname;

                            Neo4JNodeDto keyFieldNeo4JNodeDto = new Neo4JNodeDto();
                            string nodeLabel = "SAP:TIMEFIELD:PROPERTYFIELD";
                            // Check if Date Field is Event Related
                            if (fieldname.Equals("AEZET") || fieldname.Equals("ERZET"))
                            {
                                nodeLabel += ":EVENTTIME";
                            }

                            // Set the Node Label
                            keyFieldNeo4JNodeDto.Label = nodeLabel;
                            keyFieldNeo4JNodeDto.Name = keyFieldName;

                            // Unify name with the Role Name
                            keyFieldNeo4JNodeDto.Alias = fieldname;

                            keyFieldNeo4JNodeDto.AddProperty("FIELDNAME", fieldname);
                            keyFieldNeo4JNodeDto.AddProperty("ROLLNAME", rollname);
                            keyFieldNeo4JNodeDto.AddProperty("DOMNAME", domname);
                            keyFieldNeo4JNodeDto.AddProperty("DATATYPE", datatype);
                            keyFieldNeo4JNodeDto.AddProperty("Description", description);
                            keyFieldNeo4JNodeDto.AddProperty("Position", position);

                            fieldNodes.Add(keyFieldNeo4JNodeDto);
                        }
                    }
                }
            }

            return fieldNodes;
        }

        public List<SAPTableRelationDto> GetSAPTimeFieldRelations()
        {
            List<SAPTableRelationDto> dateFieldRelations = new List<SAPTableRelationDto>();

            OracleCommand cmd = new OracleCommand();
            // Go through all Ref Tables and Check Tables
            // Optimierte SQL Query

            //TODO: Only not empty Tables?
            cmd.CommandText =
                "Select TABNAME,CHECKTABLE,REFTABLE,REFFIELD,FIELDNAME,KEYFLAG,DATATYPE,DOMNAME,ROLLNAME from SAPSR3.dd03vv where tabname IN(Select Tabname from SAPSR3.DD02V A join SAPSR3.DBSTATTORA B on A.tabname = B.tname where tabclass='TRANSP' and ddlanguage = 'E' and contflag = 'A' and NROWS > 0) and tabclass='TRANSP' and datatype = 'TIMS'";

            cmd.Connection = con;
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }

            // Evtl erst alle Rows durchgehen und abspeichern und dann alle Tabellen aus dem ersten Abruf durchgehen? 
            OracleDataReader dr = cmd.ExecuteReader();
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    // Tablename
                    string tabname = dr["TABNAME"].ToString();

                    if (tabname.Equals("EKKO"))
                    {
                        var c = 1;
                    }

                    // Checktable
                    var checkTableName = dr["CHECKTABLE"].ToString();
                    // Ref Infos
                    var refTableName = dr["REFTABLE"].ToString();
                    var refFieldName = dr["REFFIELD"].ToString();
                    // Current Field
                    string fieldname = dr["FIELDNAME"].ToString();
                    // Field is Key ?
                    bool isKeyField = dr["KEYFLAG"].ToString().Equals("X");

                    string datatype = dr["DATATYPE"].ToString();
                    string domname = dr["DOMNAME"].ToString();
                    string rollname = dr["ROLLNAME"].ToString();

                    string keyFieldName = "";
                    if (String.IsNullOrWhiteSpace(rollname))
                    {
                        keyFieldName = fieldname;
                    }
                    else
                    {
                        keyFieldName = fieldname + ":" + rollname;
                        // keyFieldName = fieldname + ": " + domname;
                    }

                    keyFieldName += ":" + tabname;

                    string relationshipType = "IS_DEFINED_BY_TIMEFIELD";
                    // Check if Date Field is Event Related
                    if (fieldname.Equals("AEZET") || fieldname.Equals("ERZET"))
                    {
                        relationshipType = "IS_DEFINED_BY_EVENTTIMEFIELD";
                    }

                    // Add the relation between table and Key Field
                    SAPTableRelationDto sapTableRelation = new SAPTableRelationDto
                    {
                        RelationshipType = relationshipType,
                        Node1Name = tabname,
                        Node2Name = keyFieldName
                    };

                    dateFieldRelations.Add(sapTableRelation);
                }
            }

            return dateFieldRelations;
        }

        /// <summary>
        /// Returns the Fields with Relation to an username/resource.
        /// </summary>
        /// <returns></returns>
        public List<Neo4JNodeDto> GetSAPResourceFieldNodes()
        {
            List<Neo4JNodeDto> fieldNodes = new List<Neo4JNodeDto>();

            OracleConnection con = this.ConnectToOracle();
            OracleCommand getTablesCommand = new OracleCommand();

            getTablesCommand.CommandText =
                "Select DISTINCT t1.Tabname, t1.fieldname, t1.rollname, t1.domname, t1.DATATYPE, t1.POSITION, t2.ddtext from SAPSR3.dd03vv t1 join Sapsr3.DD04V t2 on t1.rollname = t2.rollname and t1.domname = t2.domname where tabname IN(Select Tabname from SAPSR3.DD02V A join SAPSR3.DBSTATTORA B on A.tabname = B.tname where tabclass='TRANSP' and ddlanguage = 'E' and contflag = 'A' and NROWS > 0) and t2.ddlanguage = 'E' and t1.tabclass='TRANSP' and (t1.domname = 'USNAM' or t1.rollname = 'USNAM') and (t1.keyflag != 'X')";
            getTablesCommand.Connection = con;
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }

            OracleDataReader dr = getTablesCommand.ExecuteReader();
            if (dr.HasRows)
            {
                // Füge nun alle gefundenen Tabellen der Liste hinzu.
                while (dr.Read())
                {
                    // Current Field
                    string fieldname = dr["FIELDNAME"].ToString();


                    // MANDT ist in jeder Tabelle vorhanden daher schlecht für das Clustern. 
                    if (!fieldname.Contains("MANDT") && !fieldname.Contains(".INCLUDE"))
                    {
                        string tabname = dr["TABNAME"].ToString();
                        string rollname = dr["ROLLNAME"].ToString();

                        string domname = dr["domname"].ToString();
                        string datatype = dr["DATATYPE"].ToString();
                        int position = int.Parse(dr["POSITION"].ToString());

                        string description = dr["ddtext"].ToString();


                        if (true)
                        {
                            string keyFieldName = "";
                            if (String.IsNullOrWhiteSpace(rollname))
                            {
                                keyFieldName = fieldname;
                            }
                            else
                            {
                                keyFieldName = fieldname + ":" + rollname;
                            }

                            keyFieldName += ":" + tabname;

                            Neo4JNodeDto keyFieldNeo4JNodeDto = new Neo4JNodeDto();
                            string nodeLabel = "SAP:RESOURCEFIELD:PROPERTYFIELD";


                            // Check if Resource Field is Event Related
                            /*
                            if (fieldname.Equals("AEDAT") || fieldname.Equals("ERDAT"))
                            {
                                nodeLabel += ":EVENTDATE";
                            }
                            */

                            // Set the Node Label
                            keyFieldNeo4JNodeDto.Label = nodeLabel;
                            keyFieldNeo4JNodeDto.Name = keyFieldName;

                            // Unify name with the Role Name
                            keyFieldNeo4JNodeDto.Alias = fieldname;

                            keyFieldNeo4JNodeDto.AddProperty("FIELDNAME", fieldname);
                            keyFieldNeo4JNodeDto.AddProperty("ROLLNAME", rollname);
                            keyFieldNeo4JNodeDto.AddProperty("DOMNAME", domname);
                            keyFieldNeo4JNodeDto.AddProperty("DATATYPE", datatype);
                            keyFieldNeo4JNodeDto.AddProperty("Description", description);
                            keyFieldNeo4JNodeDto.AddProperty("Position", position);

                            fieldNodes.Add(keyFieldNeo4JNodeDto);
                        }
                    }
                }
            }

            return fieldNodes;
        }

        /// <summary>
        /// Return the relation between username/resource related fields and the corresponding tables.
        /// </summary>
        /// <returns></returns>
        public List<SAPTableRelationDto> GetSAPResourceFieldRelations()
        {
            List<SAPTableRelationDto> resourceFieldRelations = new List<SAPTableRelationDto>();

            OracleCommand cmd = new OracleCommand();
            cmd.CommandText =
                "Select TABNAME,CHECKTABLE,REFTABLE,REFFIELD,FIELDNAME,KEYFLAG,DATATYPE,DOMNAME,ROLLNAME from SAPSR3.dd03vv where tabname IN(Select Tabname from SAPSR3.DD02V A join SAPSR3.DBSTATTORA B on A.tabname = B.tname where tabclass='TRANSP' and ddlanguage = 'E' and contflag = 'A' and NROWS > 0) and tabclass='TRANSP' and (DOMNAME='USNAM' or Rollname = 'USNAM')";

            cmd.Connection = con;
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }

            OracleDataReader dr = cmd.ExecuteReader();
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    // Tablename
                    string tabname = dr["TABNAME"].ToString();

                    if (tabname.Equals("EKKO"))
                    {
                        var c = 1;
                    }

                    // Checktable
                    var checkTableName = dr["CHECKTABLE"].ToString();
                    // Ref Infos
                    var refTableName = dr["REFTABLE"].ToString();
                    var refFieldName = dr["REFFIELD"].ToString();
                    // Current Field
                    string fieldname = dr["FIELDNAME"].ToString();
                    // Field is Key ?
                    bool isKeyField = dr["KEYFLAG"].ToString().Equals("X");

                    string datatype = dr["DATATYPE"].ToString();
                    string domname = dr["DOMNAME"].ToString();
                    string rollname = dr["ROLLNAME"].ToString();

                    string keyFieldName = "";
                    if (String.IsNullOrWhiteSpace(rollname))
                    {
                        keyFieldName = fieldname;
                    }
                    else
                    {
                        keyFieldName = fieldname + ":" + rollname;
                        // keyFieldName = fieldname + ": " + domname;
                    }

                    keyFieldName += ":" + tabname;

                    string relationshipType = "IS_DEFINED_BY_RESOURCEFIELD";
                    // Check if Resource Field is Event Related
                    /*
                    if (fieldname.Equals("AEDAT") || fieldname.Equals("ERDAT"))
                    {
                        relationshipType = "IS_DEFINED_BY_EVENTDATEFIELD";
                    }
                    */

                    // Add the relation between table and Resource Field
                    SAPTableRelationDto sapTableRelation = new SAPTableRelationDto
                    {
                        RelationshipType = relationshipType,
                        Node1Name = tabname,
                        Node2Name = keyFieldName
                    };

                    resourceFieldRelations.Add(sapTableRelation);
                }
            }

            return resourceFieldRelations;
        }

        #endregion


        /// <summary>
        /// Retruns all other Fields.
        /// </summary>
        /// <returns></returns>
        public List<Neo4JNodeDto> GetSAPFieldNodes()
        {
            List<Neo4JNodeDto> fieldNodes = new List<Neo4JNodeDto>();

            OracleConnection con = this.ConnectToOracle();
            OracleCommand getTablesCommand = new OracleCommand();

            getTablesCommand.CommandText =
                "Select DISTINCT t1.Tabname, t1.fieldname, t1.rollname, t1.domname, t1.DATATYPE, t1.POSITION, t2.ddtext from SAPSR3.dd03vv t1 join Sapsr3.DD04V t2 on t1.rollname = t2.rollname and t1.domname = t2.domname where tabname IN(Select Tabname from SAPSR3.DD02V A join SAPSR3.DBSTATTORA B on A.tabname = B.tname where tabclass='TRANSP' and ddlanguage = 'E' and contflag = 'A' and NROWS > 0) and t2.ddlanguage = 'E' and t1.tabclass='TRANSP' and t1.datatype != 'DATS' and t1.datatype != 'TIMS' and (t1.keyflag != 'X')";
            getTablesCommand.Connection = con;
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }

            OracleDataReader dr = getTablesCommand.ExecuteReader();
            if (dr.HasRows)
            {
                // Füge nun alle gefundenen Tabellen der Liste hinzu.
                while (dr.Read())
                {
                    // Current Field
                    string fieldname = dr["FIELDNAME"].ToString();

                    // MANDT ist in jeder Tabelle vorhanden daher schlecht für das Clustern. 
                    if (!fieldname.Contains("MANDT") && !fieldname.Contains(".INCLUDE"))
                    {
                        string tabname = dr["TABNAME"].ToString();
                        string rollname = dr["ROLLNAME"].ToString();
                        string domname = dr["domname"].ToString();
                        string datatype = dr["DATATYPE"].ToString();
                        int position = int.Parse(dr["POSITION"].ToString());

                        string description = dr["ddtext"].ToString();

                        //Sortiere nicht relevante Datentypen raus
                        if (
                            true) //!datatype.Equals("CLNT") && !datatype.Equals("LANG") && !datatype.Equals("DATS") && !exludedDoms.Contains(domname))
                        {
                            string keyFieldName = "";
                            if (String.IsNullOrWhiteSpace(rollname))
                            {
                                keyFieldName = fieldname;
                            }
                            else
                            {
                                keyFieldName = fieldname + ":" + rollname;
                            }

                            keyFieldName += ":" + tabname;

                            Neo4JNodeDto keyFieldNeo4JNodeDto = new Neo4JNodeDto();
                            string nodeLabel = "SAP:PROPERTYFIELD";

                            // Set the Node Label
                            keyFieldNeo4JNodeDto.Label = nodeLabel;
                            keyFieldNeo4JNodeDto.Name = keyFieldName;

                            // Unify name with the Role Name
                            keyFieldNeo4JNodeDto.Alias = fieldname;

                            keyFieldNeo4JNodeDto.AddProperty("FIELDNAME", fieldname);
                            keyFieldNeo4JNodeDto.AddProperty("ROLLNAME", rollname);
                            keyFieldNeo4JNodeDto.AddProperty("DOMNAME", domname);
                            keyFieldNeo4JNodeDto.AddProperty("DATATYPE", datatype);
                            keyFieldNeo4JNodeDto.AddProperty("Description", description);
                            keyFieldNeo4JNodeDto.AddProperty("Position", position);

                            fieldNodes.Add(keyFieldNeo4JNodeDto);
                        }
                    }
                }
            }

            return fieldNodes;
        }


        public List<SAPTableRelationDto> GetSAPFieldRelations()
        {
            List<SAPTableRelationDto> dateFieldRelations = new List<SAPTableRelationDto>();

            OracleCommand cmd = new OracleCommand();
            // Go through all Ref Tables and Check Tables
            // Optimierte SQL Query

            //TODO: Only not empty Tables?
            cmd.CommandText =
                "Select TABNAME,CHECKTABLE,REFTABLE,REFFIELD,FIELDNAME,KEYFLAG,DATATYPE,DOMNAME,ROLLNAME from SAPSR3.dd03vv where tabname IN(Select Tabname from SAPSR3.DD02V A join SAPSR3.DBSTATTORA B on A.tabname = B.tname where tabclass='TRANSP' and ddlanguage = 'E' and contflag = 'A' and NROWS > 0) and tabclass='TRANSP' and datatype != 'DATS' and datatype != 'TIMS' and (keyflag != 'X')";

            cmd.Connection = con;
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }

            // Evtl erst alle Rows durchgehen und abspeichern und dann alle Tabellen aus dem ersten Abruf durchgehen? 
            OracleDataReader dr = cmd.ExecuteReader();
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    // Tablename
                    string tabname = dr["TABNAME"].ToString();

                    if (tabname.Equals("EKKO"))
                    {
                        var c = 1;
                    }

                    // Checktable
                    var checkTableName = dr["CHECKTABLE"].ToString();
                    // Ref Infos
                    var refTableName = dr["REFTABLE"].ToString();
                    var refFieldName = dr["REFFIELD"].ToString();
                    // Current Field
                    string fieldname = dr["FIELDNAME"].ToString();
                    // Field is Key ?
                    bool isKeyField = dr["KEYFLAG"].ToString().Equals("X");

                    string datatype = dr["DATATYPE"].ToString();
                    string domname = dr["DOMNAME"].ToString();
                    string rollname = dr["ROLLNAME"].ToString();

                    string keyFieldName = "";
                    if (String.IsNullOrWhiteSpace(rollname))
                    {
                        keyFieldName = fieldname;
                    }
                    else
                    {
                        keyFieldName = fieldname + ":" + rollname;
                        // keyFieldName = fieldname + ": " + domname;
                    }

                    keyFieldName += ":" + tabname;

                    string relationshipType = "IS_DEFINED_BY_FIELD";

                    // Add the relation between table and Field
                    SAPTableRelationDto sapTableRelation = new SAPTableRelationDto
                    {
                        RelationshipType = relationshipType,
                        Node1Name = tabname,
                        Node2Name = keyFieldName
                    };

                    dateFieldRelations.Add(sapTableRelation);
                }
            }

            return dateFieldRelations;
        }

        #endregion


        public List<Neo4JNodeDto> GetTCodeNodes()
        {
            List<Neo4JNodeDto> sapTCodeNodes = new List<Neo4JNodeDto>();

            OracleConnection con = this.ConnectToOracle();
            OracleCommand getTablesCommand = new OracleCommand();

            // Die Tabelle DD02V enthält zusätzlich noch die Klartextbesschreibungen.
            // Hierbei wählen wir nur die englischen Beschreibungen
            // getTablesCommand.CommandText = "Select TABNAME, DDTEXT, TABCLASS, APPLCLASS, CONTFLAG from SAPSR3.DD02V where tabclass='TRANSP' and contflag = 'A' and ddlanguage = 'E'";


            // SQL Command with Row Count
            getTablesCommand.CommandText =
                "select DISTINCT C.TCODE, C.PGMNA, D.TTEXT from SAPSR3.D010TAB A JOIN SAPSR3.DD02V B on A.tabname = B.tabname JOIN SAPSR3.TSTC C on A.MASTER = C.PGMNA JOIN SAPSR3.TSTCT D on C.TCODE = D.TCODE where B.tabclass = 'TRANSP' and ddlanguage = 'E' and D.SPRSL = 'E'";


            getTablesCommand.Connection = con;
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }

            OracleDataReader dr = getTablesCommand.ExecuteReader();
            if (dr.HasRows)
            {
                // Füge nun alle gefundenen Tabellen der Liste hinzu.
                while (dr.Read())
                {
                    var TCODE = dr["TCODE"].ToString();
                    var programName = dr["PGMNA"].ToString();
                    var description = dr["TTEXT"].ToString();

                    Neo4JNodeDto neo4JNodeDto = new Neo4JNodeDto();
                    neo4JNodeDto.Label = "SAP:TCODE";

                    neo4JNodeDto.Name = TCODE;
                    neo4JNodeDto.Alias = TCODE;
                    neo4JNodeDto.AddProperty("Description", description);

                    sapTCodeNodes.Add(neo4JNodeDto);
                }
            }

            return sapTCodeNodes;
        }

        public List<SAPTableRelationDto> GetTCodeTableRelations()
        {
            List<SAPTableRelationDto> tCodeRelations = new List<SAPTableRelationDto>();

            OracleConnection con = this.ConnectToOracle();
            OracleCommand getTablesCommand = new OracleCommand();

            // Die Tabelle DD02V enthält zusätzlich noch die Klartextbesschreibungen.
            // Hierbei wählen wir nur die englischen Beschreibungen
            // getTablesCommand.CommandText = "Select TABNAME, DDTEXT, TABCLASS, APPLCLASS, CONTFLAG from SAPSR3.DD02V where tabclass='TRANSP' and contflag = 'A' and ddlanguage = 'E'";


            // SQL Command with Row Count
            getTablesCommand.CommandText =
                "select A.MASTER, A.TABNAME, B.DDTEXT, C.TCODE, D.TTEXT from SAPSR3.D010TAB A JOIN SAPSR3.DD02V B on A.tabname = B.tabname JOIN SAPSR3.TSTC C on A.MASTER = C.PGMNA JOIN SAPSR3.TSTCT D on C.TCODE = D.TCODE where B.tabclass = 'TRANSP' and ddlanguage = 'E' and D.SPRSL = 'E' and A.Tabname IN(Select Tabname from SAPSR3.DD02V A join SAPSR3.DBSTATTORA B on A.tabname = B.tname where tabclass='TRANSP' and ddlanguage = 'E' and contflag = 'A' and NROWS > 0)";


            getTablesCommand.Connection = con;
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }

            OracleDataReader dr = getTablesCommand.ExecuteReader();
            if (dr.HasRows)
            {
                // Füge nun alle gefundenen Tabellen der Liste hinzu.
                while (dr.Read())
                {
                    var tableName = dr["TABNAME"].ToString();
                    var tcodeName = dr["TCODE"].ToString();

                    // Add the relation between table and Key Field
                    SAPTableRelationDto tCodeRelation = new SAPTableRelationDto
                    {
                        RelationshipType = "IS_CHANGED_IN",
                        Node1Name = tcodeName,
                        Node2Name = tableName
                    };

                    tCodeRelations.Add(tCodeRelation);
                }
            }

            return tCodeRelations;
        }


        public SAPNeo4JGraphDto GetObjectTypesData()
        {
            SAPNeo4JGraphDto sapGraphDto = new SAPNeo4JGraphDto();

            OracleConnection con = this.ConnectToOracle();

            // Get all Object Types from SAP
            OracleCommand getObjectTypesCommand = new OracleCommand();

            // Combined with CDPOS Table for Change Count.
            getObjectTypesCommand.CommandText =
                "Select objectclas, Count(*) as count from(select distinct objectclas, changenr from " +
                this.table_prefix + "cdpos where mandant = '800') group by objectclas ORDER BY Count(*) DESC";

            getObjectTypesCommand.Connection = con;
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }

            OracleDataReader dr = getObjectTypesCommand.ExecuteReader();
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    var objectName = dr["objectclas"].ToString();
                    var objectCount = dr["count"].ToString();

                    Neo4JNodeDto foundObject = sapGraphDto.Nodes.FirstOrDefault(x => x.Name.Equals(objectName));
                    if (foundObject == null)
                    {
                        Neo4JNodeDto neo4JNodeDto = new Neo4JNodeDto();
                        neo4JNodeDto.Label = "SAP:ObjectType";
                        neo4JNodeDto.Name = objectName;
                        neo4JNodeDto.Alias = objectName;
                        neo4JNodeDto.AddProperty("ENTRYCOUNT", int.Parse(objectCount));

                        sapGraphDto.Nodes.Add(neo4JNodeDto);
                    }
                }
            }

            // Create Connections between objects and tables.
            OracleCommand getObjectRelationsCommand = new OracleCommand();
            getObjectRelationsCommand.CommandText =
                "Select t1.object, t2.tabname, t2.count from " + this.table_prefix +
                "TCDOB t1 JOIN  (Select objectclas, tabname, Count(*) as count from (select distinct objectclas, tabname, changenr from " +
                this.table_prefix +
                "cdpos where mandant='800') group by objectclas, tabname ORDER BY Count(*) DESC) t2 on t1.object = t2.objectclas and t1.tabname = t2.tabname";

            getObjectRelationsCommand.Connection = con;
            // Check current Connection State
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }

            dr = getObjectRelationsCommand.ExecuteReader();
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    var objectName = dr["object"].ToString();
                    var tableName = dr["tabname"].ToString();
                    var count = dr["count"].ToString();


                    SAPTableRelationDto relationDto = new SAPTableRelationDto();
                    relationDto.Node1Name = objectName;
                    relationDto.Node2Name = tableName;

                    // Add the info how often this Entry is changed.
                    relationDto.AddProperty("ENTRYCOUNT", int.Parse(count));
                    relationDto.RelationshipType = "IS_CREATED_IN";

                    sapGraphDto.Edges.Add(relationDto);
                }
            }


            // Add Textual Description to Objects
            OracleCommand getObjectDescCommand = new OracleCommand();
            getObjectDescCommand.CommandText =
                "Select t1.objectclas, t2.obtext from(select distinct objectclas, changenr from " + this.table_prefix +
                "cdpos where mandant = '800') t1 join " + this.table_prefix +
                "TCDOBT t2 on t1.objectclas = t2.Object where t2.spras = 'E' group by objectclas, obtext";

            getObjectDescCommand.Connection = con;
            // Check current Connection State
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }

            Dictionary<string, string> objectDescDictionary = new Dictionary<string, string>();

            dr = getObjectDescCommand.ExecuteReader();
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    var objectName = dr["objectclas"].ToString();
                    var objectDesc = dr["OBTEXT"].ToString();

                    objectDescDictionary.Add(objectName, objectDesc);
                }
            }

            // Set the description for all Objects
            foreach (Neo4JNodeDto objectType in sapGraphDto.Nodes)
            {
                var objectDesc = objectType.Name;

                if (objectDescDictionary.ContainsKey(objectDesc))
                {
                    objectDesc = objectDescDictionary[objectType.Name];
                }

                objectType.AddProperty("Description", objectDesc);
            }

            // Return the filled SAPGraphDTO
            return sapGraphDto;
        }

        public SAPNeo4JCompleteGraphDto GetFunctionalModuleData()
        {
            SAPNeo4JCompleteGraphDto neo4JCompleteGraphDto = new SAPNeo4JCompleteGraphDto();

            OracleConnection con = this.ConnectToOracle();

            // Get all Functional Modules from SAP
            OracleCommand getTablesCommand = new OracleCommand();

            // Get all Application Areas in English
            getTablesCommand.CommandText = "Select * from " + this.table_prefix + "TAPLT where sprsl = 'E'";
            getTablesCommand.Connection = con;
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }

            OracleDataReader dr = getTablesCommand.ExecuteReader();
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    var className = dr["APPL"].ToString();
                    var classText = dr["ATEXT"].ToString();

                    Neo4JNodeDto classNode = new Neo4JNodeDto
                    {
                        Name = className + ":" + classText,
                        Alias = classText,
                        Label = "SAP:MODULE"
                    };

                    classNode.AddProperty("APPL", className);
                    classNode.AddProperty("Description", classText);

                    neo4JCompleteGraphDto.TableNodes.Add(classNode);
                }
            }

            OracleCommand getClassRelationsCommand = new OracleCommand();

            getClassRelationsCommand.CommandText =
                "Select A.AREA, A.FUNCNAME, B.APPL, C.ATEXT from " + this.table_prefix +
                "ENLFDIR A join SAPSR3.TFDIR B on A.FUNCNAME = B.FUNCNAME join " + this.table_prefix +
                "TAPLT C on B.APPL = C.APPL where area IN(Select Tabname from " + this.table_prefix +
                "DD02V where tabclass='TRANSP' and ddlanguage = 'E' and contflag = 'A') and B.APPL <>' ' and C.sprsl = 'E'";
            getClassRelationsCommand.Connection = con;
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }

            dr = getClassRelationsCommand.ExecuteReader();
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                }
            }

            return neo4JCompleteGraphDto;
        }

        public SAPNeo4JGraphDto GetApplicationClassData()
        {
            SAPNeo4JGraphDto neo4JCompleteGraphDto = new SAPNeo4JGraphDto();

            OracleConnection con = this.ConnectToOracle();

            // Get all Functional Modules from SAP
            OracleCommand getTablesCommand = new OracleCommand();

            // Get all Application Areas in English
            getTablesCommand.CommandText =
                "Select FCTR_ID, NAME, PS_POSID from " + this.table_prefix + "DF14VD where langu = 'E'";
            getTablesCommand.Connection = con;
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }

            OracleDataReader dr = getTablesCommand.ExecuteReader();
            // Build the application Hierarchy
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    var classID = dr["FCTR_ID"].ToString();
                    var classname = dr["NAME"].ToString();
                    var posID = dr["PS_POSID"].ToString();


                    Neo4JNodeDto classNode = new Neo4JNodeDto
                    {
                        Name = posID,
                        Alias = classname,
                        Label = "SAP:APPLICATIONCLASS"
                    };

                    classNode.AddProperty("FCTR_ID", classID);
                    classNode.AddProperty("Description", posID + " " + classname);
                    classNode.AddProperty("PS_POSID", posID);

                    neo4JCompleteGraphDto.Nodes.Add(classNode);

                    // Get the current Application Path
                    string[] currentPath = posID.Split('-');
                    if (currentPath.Length > 1)
                    {
                        if (posID.Equals("PY-IN-NT"))
                        {
                        }

                        string parentPath = "";
                        for (int i = 0; i < currentPath.Length - 1; i++)
                        {
                            parentPath += currentPath[i] + "-";
                        }

                        parentPath = parentPath.Substring(0, parentPath.Length - 1);

                        SAPTableRelationDto relationDto = new SAPTableRelationDto();
                        relationDto.Node1Name = parentPath;
                        relationDto.Node2Name = posID;
                        relationDto.RelationshipType = "IS_SUBMODULE_IN";

                        neo4JCompleteGraphDto.Edges.Add(relationDto);
                    }
                }
            }

            OracleCommand getClassRelationsCommand = new OracleCommand();

            getClassRelationsCommand.CommandText =
                "Select TABNAME, DDTEXT, TABCLASS, APPLCLASS, CONTFLAG, NROWS, D.Name, D.PS_POSID from " +
                this.table_prefix + "DD02V A join " + this.table_prefix + "DBSTATTORA B on A.tabname = B.tname join " +
                this.table_prefix + "TDEVC C on A.APPLCLASS = C.devclass join " + this.table_prefix +
                "DF14VD D on D.FCTR_ID = C.component  where tabclass='TRANSP' and ddlanguage = 'E' and contflag = 'A' and NROWS > 0 and D.langu = 'E'";
            getClassRelationsCommand.Connection = con;
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }

            dr = getClassRelationsCommand.ExecuteReader();
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    var tabname = dr["TABNAME"].ToString();
                    var posID = dr["PS_POSID"].ToString();


                    SAPTableRelationDto relationDto = new SAPTableRelationDto();
                    relationDto.Node1Name = tabname;
                    relationDto.Node2Name = posID;
                    relationDto.RelationshipType = "IS_LOCATED_IN";

                    neo4JCompleteGraphDto.Edges.Add(relationDto);
                }
            }

            return neo4JCompleteGraphDto;
        }

        public SAPNeo4JGraphDto GetSAPDomainGraph(List<Neo4JNodeDto> primaryKeyNodes)
        {
            SAPNeo4JGraphDto sapDomainGraph = new SAPNeo4JGraphDto();

            OracleConnection con = this.ConnectToOracle();
            OracleCommand getTablesCommand = new OracleCommand();

            // Die Tabelle DD02V enthält zusätzlich noch die Klartextbesschreibungen.
            // Hierbei wählen wir nur die englischen Beschreibungen
            // getTablesCommand.CommandText = "Select TABNAME, DDTEXT, TABCLASS, APPLCLASS, CONTFLAG from SAPSR3.DD02V where tabclass='TRANSP' and contflag = 'A' and ddlanguage = 'E'";


            // Get all domains that are present in the current avaiable table set and the coressponding keys.
            getTablesCommand.CommandText =
                "SELECT domname, datatype, entitytab, ddtext from " + this.table_prefix +
                "DD01VV where ddlanguage = 'E' and Domname IN(Select DISTINCT domname from " + this.table_prefix +
                "dd03vv where tabname IN(Select Tabname from " + this.table_prefix + "DD02V A join " +
                this.table_prefix +
                "DBSTATTORA B on A.tabname = B.tname where tabclass='TRANSP' and ddlanguage = 'E' and contflag = 'A' and NROWS > 0) and tabclass='TRANSP' and (keyflag = 'X'))";

            getTablesCommand.Connection = con;
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }

            OracleDataReader dr = getTablesCommand.ExecuteReader();
            if (dr.HasRows)
            {
                // Füge nun alle gefundenen Domainen der Liste hinzu.
                while (dr.Read())
                {
                    var domainName = dr["domname"].ToString();
                    var datatype = dr["datatype"].ToString();
                    var entityTab = dr["entitytab"].ToString();
                    var description = dr["ddtext"].ToString();

                    if (domainName.Equals("EBELN"))
                    {
                        var a = "b";
                    }

                    Neo4JNodeDto neo4JNodeDto = new Neo4JNodeDto();
                    neo4JNodeDto.Label = "SAP:DOMAIN";
                    neo4JNodeDto.Name = domainName;
                    neo4JNodeDto.Alias = domainName;

                    neo4JNodeDto.AddProperty("Datatype", datatype);
                    neo4JNodeDto.AddProperty("Description", description);
                    neo4JNodeDto.AddProperty("EntityTab", entityTab);

                    sapDomainGraph.Nodes.Add(neo4JNodeDto);


                    List<Neo4JNodeDto> primaryKeysInDom = primaryKeyNodes.Where(x =>
                            x.Properties.FirstOrDefault(y => y.PropertyName.Equals("DOMNAME")).Value.Equals(domainName))
                        .ToList();
                    foreach (Neo4JNodeDto primaryKey in primaryKeysInDom)
                    {
                        SAPTableRelationDto sapKeyRelationDto = new SAPTableRelationDto();

                        sapKeyRelationDto.Node1Name = primaryKey.Name;
                        sapKeyRelationDto.Node2Name = domainName;
                        sapKeyRelationDto.RelationshipType = "IS_IN_DOMAIN";

                        sapDomainGraph.Edges.Add(sapKeyRelationDto);
                    }

                    if (!String.IsNullOrWhiteSpace(entityTab))
                    {
                        SAPTableRelationDto sapTableRelationDto = new SAPTableRelationDto();

                        sapTableRelationDto.Node1Name = entityTab;
                        sapTableRelationDto.Node2Name = domainName;
                        sapTableRelationDto.RelationshipType = "IS_ENTITYTAB_OF";

                        sapDomainGraph.Edges.Add(sapTableRelationDto);
                    }
                }
            }


            return sapDomainGraph;
        }
    }
}