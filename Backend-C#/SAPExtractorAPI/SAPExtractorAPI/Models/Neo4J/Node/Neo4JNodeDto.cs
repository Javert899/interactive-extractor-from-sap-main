using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace SAPExtractorAPI.Models.Neo4J
{
    public class GenNode
    {
        public long Id { get; set; }
        public IEnumerable<String> Labels { get; set; }
        public Dictionary<String, String> Properties { get; set; }
    }

    [Serializable, DataContract]
    public class Neo4JNodeDto
    {
        public Neo4JNodeDto()
        {
            Properties = new List<Neo4jNodePropertyDto>();
            ChildNodes = new List<Neo4JNodeDto>();
        }
        /// <summary>
        /// Copy Konstruktor
        /// </summary>
        /// <param name="copy">Das zu kopierende Objekt</param>
        public Neo4JNodeDto(Neo4JNodeDto copy)
        {
            Properties = new List<Neo4jNodePropertyDto>();
            Id = copy.Id;
            Name = copy.Name;
            Label = copy.Label;

            foreach (Neo4jNodePropertyDto property in copy.Properties)
            {
                Properties.Add(new Neo4jNodePropertyDto(property));
            }
        }

        /// <summary>
        /// Add a new property to the Table Dto. Existing Property will be updated.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="propertyValue">The value of the property.</param>
        public void AddProperty(string propertyName, object propertyValue)
        {
            Neo4jNodePropertyDto foundProperty =
                this.Properties.FirstOrDefault(x => x.PropertyName.Equals(propertyName));
            if (foundProperty != null)
            {
                foundProperty.Value = propertyValue;
            }
            else
            {
                Neo4jNodePropertyDto nodeProperty = new Neo4jNodePropertyDto
                {
                    NodeId = this.Id,
                    PropertyName = propertyName,
                    Value = propertyValue
                };

                this.Properties.Add(nodeProperty);
            }
        }

        /// <summary>
        /// Id des Knotens (NICHT die interne Id von Neo4j)
        /// Muss beim hinzufügen manuell gesetzt werden (GetNextId)
        /// </summary>
        [DataMember]
        public long Id { get; set; }
        /// <summary>
        /// interne Id des Knotens von Neo4J
        /// </summary>
        [DataMember]
        public long referenceId { get; set; }
        /// <summary>
        /// Name bzw. Bezeichnung des Knotens
        /// </summary>
        [DataMember]
        public string Name { get; set; }
        /// <summary>
        /// Alle Label durch ':' separiert
        /// </summary>
        [DataMember]
        public string Label { get; set; }

        /// <summary>
        /// Alias
        /// </summary>
        [DataMember]
        public string Alias { get; set; }

        /// <summary>
        /// Alle weiteren dynamischen Eigenschaften
        /// </summary>
        [DataMember]
        public virtual List<Neo4jNodePropertyDto> Properties { get; set; }

        [DataMember]
        public List<Neo4JNodeDto> ChildNodes { get; set; }

        public List<string> Labels
        {
            get
            {
                if (Label == null)
                {
                    return new List<string>();
                }
                else return Label.Split(':').ToList();
            }
            set
            {
                Label = string.Join(":", value);
            }
        }

        public string GetPropertyValue(string propertyName)
        {
            Neo4jNodePropertyDto property = Properties.FirstOrDefault(x => x.PropertyName == propertyName);
            if (property != null)
                return property.Value.ToString();
            return string.Empty;
        }
    }
}
