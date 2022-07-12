using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SAPExtractorAPI.Models.Helper
{
    /// <summary>
    /// Enum um zu bestimmen, was passieren soll, falls ein Datensatz schon vorhanden ist
    /// </summary>
    public enum ImportType
    {
        /// <summary>
        /// Datensatz ueberschreiben
        /// </summary>
        Override = 0,
        /// <summary>
        /// Neuen Datensatz NICHT einpflegen
        /// </summary>
        Ignore = 1,
        /// <summary>
        /// Noch nicht vorhandene Eigenschaften des neuen Datensatzes zum alten Datensatz hinzufuegen
        /// </summary>
        Complete = 2
    }
}