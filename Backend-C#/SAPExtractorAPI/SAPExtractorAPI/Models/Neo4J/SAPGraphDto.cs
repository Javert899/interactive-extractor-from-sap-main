using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace SAPExtractorAPI.Models.Neo4J
{
    [DataContract]
    public class SAPNeo4JCompleteGraphDto
    {
        public SAPNeo4JCompleteGraphDto()
        {
            this.TableNodes = new List<Neo4JNodeDto>();
            this.TableKeyFieldNodes = new List<Neo4JNodeDto>();
            this.TableDateFieldNodes = new List<Neo4JNodeDto>();
            this.TableFieldNodes = new List<Neo4JNodeDto>();

            this.TableEdges = new List<SAPTableRelationDto>();
            this.TableKeyFieldEdges = new List<SAPTableRelationDto>();
            this.TableDateFieldEdges = new List<SAPTableRelationDto>();
            this.TableFieldEdges = new List<SAPTableRelationDto>();


            this.ApplicationClassNodes = new List<Neo4JNodeDto>();
            this.ApplicationClassEdges = new List<SAPTableRelationDto>();

            this.ObjecttypeNode = new List<Neo4JNodeDto>();
            this.ObjecttypeEdges = new List<SAPTableRelationDto>();
        }

        // Contains all relevant Tables
        [DataMember]
        public List<Neo4JNodeDto> TableNodes { get; set; }
        [DataMember]
        public List<SAPTableRelationDto> TableEdges { get; set; }

        // Represents the Primary Key Properties
        [DataMember]
        public List<Neo4JNodeDto> TableKeyFieldNodes { get; set; }
        [DataMember]
        public List<SAPTableRelationDto> TableKeyFieldEdges { get; set; }

        // Represents Date Relevant Properties of a Table
        [DataMember]
        public List<Neo4JNodeDto> TableDateFieldNodes { get; set; }
        [DataMember]
        public List<SAPTableRelationDto> TableDateFieldEdges { get; set; }

        // Represents Time Relevant Properties of a Table
        [DataMember]
        public List<Neo4JNodeDto> TableTimeFieldNodes { get; set; }
        [DataMember]
        public List<SAPTableRelationDto> TableTimeFieldEdges { get; set; }

        // Represents Resource/Username Relevant Properties of a Table
        [DataMember]
        public List<Neo4JNodeDto> TableResourceFieldNodes { get; set; }
        [DataMember]
        public List<SAPTableRelationDto> TableResourceFieldEdges { get; set; }

        // Represents other interesting Properties of a Table
        [DataMember]
        public List<Neo4JNodeDto> TableFieldNodes { get; set; }
        [DataMember]
        public List<SAPTableRelationDto> TableFieldEdges { get; set; }


        // Represents Information about the Tables Application Class
        [DataMember]
        public List<Neo4JNodeDto> ApplicationClassNodes { get; set; }
        [DataMember]
        public List<SAPTableRelationDto> ApplicationClassEdges { get; set; }


        // Contains the different Object Types in the SAP System
        [DataMember]
        public List<Neo4JNodeDto> ObjecttypeNode  { get; set; }
        [DataMember]
        public List<SAPTableRelationDto> ObjecttypeEdges { get; set; }
    }

    [DataContract]
    public class SAPNeo4JGraphDto
    {
        public SAPNeo4JGraphDto()
        {
            this.Nodes = new List<Neo4JNodeDto>();
            this.Edges = new List<SAPTableRelationDto>();
        }

        [DataMember]
        public List<Neo4JNodeDto> Nodes { get; set; }
        [DataMember]
        public List<SAPTableRelationDto> Edges { get; set; }
    }


    [DataContract]
    public class SAPGraphDto
    {
        public SAPGraphDto()
        {
            this.Expanded_tables = new List<SAPTableDto>();
            this.Edges = new List<SAPTableRelationDto>();
        }

        [DataMember]
        public List<SAPTableDto> Expanded_tables { get; set; }
        [DataMember]
        public List<SAPTableRelationDto> Edges { get; set; }
    }

    [DataContract]
    public class SAPTableExpansionDto
    {
        [DataMember]
        public List<string> Expanded_tables { get; set; }
        [DataMember]
        public List<string[]> Edges { get; set; }
    }

    [DataContract]
    public class SAPTableDto
    {
        public SAPTableDto()
        {
            TableProperties = new List<SAPTableProperty>();
            this.TableKeyFields = new List<SAPTableKeyField>();
        }

        /// <summary>
        /// Add a new property to the Table Dto. Existing Property will be updated.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="propertyValue">The value of the property.</param>
        public void AddProperty(string propertyName, object propertyValue)
        {
            SAPTableProperty foundProperty =
                this.TableProperties.FirstOrDefault(x => x.PropertyName.Equals(propertyName));
            if (foundProperty != null)
            {
                foundProperty.PropertyValue = propertyValue;
            }
            else
            {
                SAPTableProperty sapTableProperty = new SAPTableProperty
                {
                    PropertyName = propertyName,
                    PropertyValue = propertyValue
                };

                this.TableProperties.Add(sapTableProperty);
            }
        }

        [DataMember]
        public string TableName { get; set; }

        [DataMember]
        public List<SAPTableProperty> TableProperties { get; set; }

        [DataMember]
        public List<SAPTableKeyField> TableKeyFields { get; set; }
    }

    [DataContract]
    public class SAPTableKeyField
    {
        [DataMember]
        public string KeyName { get; set; }
        [DataMember]
        public string Fieldname { get; set; }
        [DataMember]
        public string Rollname { get; set; }
        [DataMember]
        public string Domname { get; set; }
        [DataMember]
        public string Checktable { get; set; }
        [DataMember]
        public string Datatype { get; set; }
    }


    [DataContract]
    public class SAPTableProperty
    {
        [DataMember]
        public string PropertyName { get; set; }
        [DataMember]
        public object PropertyValue { get; set; }
    }

    [DataContract]
    public class SAPTableRelationDto
    {
        public SAPTableRelationDto()
        {
            this.RelationProperties = new List<SAPRelationProperty>();
        }

        /// <summary>
        /// Add a new property to the Relation Dto. Existing Property will be updated.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="propertyValue">The value of the property.</param>
        public void AddProperty(string propertyName, object propertyValue)
        {
            SAPRelationProperty foundProperty = this.RelationProperties.FirstOrDefault(x => x.PropertyName.Equals(propertyName));
            if (foundProperty != null)
            {
                foundProperty.PropertyValue = propertyValue;
            }
            else
            {
                SAPRelationProperty sapTableProperty = new SAPRelationProperty
                {
                    PropertyName = propertyName,
                    PropertyValue = propertyValue
                };

                this.RelationProperties.Add(sapTableProperty);
            }
        }

        [DataMember]
        public string Node1Name { get; set; }
        [DataMember]
        public string Node2Name { get; set; }

        [DataMember]
        public string RelationshipType { get; set; }

        [DataMember]
        public List<SAPRelationProperty> RelationProperties { get; set; }
    }

    [DataContract]
    public class SAPRelationProperty
    {
        [DataMember]
        public string PropertyName { get; set; }
        [DataMember]
        public object PropertyValue { get; set; }
    }
}