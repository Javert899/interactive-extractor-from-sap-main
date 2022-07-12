using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SAPExtractorAPI.Models.Helper
{
    public class FilterDto
    {
        public FilterDto()
        {
            Condition = FilterCondition.NoFilter;
            Value = "";
        }
        public FilterCondition Condition { get; set; }
        public string Value { get; set; }


    }
    /// <summary>
    /// Gibt an was fuer eine Art Filter angewandt werden soll
    /// </summary>
    public enum FilterCondition
    {
        /// <summary>
        /// Keinen Filter verwenden
        /// </summary>
        NoFilter = 0,
        /// <summary>
        /// Startet mit Filter
        /// </summary>
        StartsWith = 1,
        /// <summary>
        /// Enthaelt
        /// </summary>
        Contains = 2,
        /// <summary>
        /// Endet mit 
        /// </summary>
        EndsWith = 3,
        /// <summary>
        /// Enthaelt nicht
        /// </summary>
        DoesNotContain = 4,
        /// <summary>
        /// Exakt gleich
        /// </summary>
        Equals = 5,
        /// <summary>
        /// Ist ungleich
        /// </summary>
        DoesNotEqual = 6,
        /// <summary>
        /// Property existiert nicht
        /// </summary>
        NotExists = 7
    }

    public class PropertyFilterDto
    {
        public PropertyFilterDto() { NodeVariableToFilterOn = string.Empty; }
        public string NodeVariableToFilterOn { get; set; }
        public FilterDto PropertyFilter { get; set; }
        public FilterDto PropertyValueFilter { get; set; }
    }
}