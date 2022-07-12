using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using SAPExtractorAPI.Models.Helper;
using SAPExtractorAPI.Models.Neo4J;
using SAPExtractorAPI.Models.Neo4J.Relation;

namespace SAPExtractorAPI.Lib.Neo4JBaseRepository
{
    public interface INeo4JBaseRepository
    {
        /*
        #region Neo4j Node

        #region Get

        /// <summary>
        /// Gibt die gefilterten Knoten zurück
        /// </summary>
        /// <param name="skip">Wie viele Knoten beim Ergebnis übersprungen werden</param>
        /// <param name="limit">Wie viele Knoten zurückgegeben werden sollen</param>
        /// <param name="total">Wie viele Knoten insgesamt im gefilterten Ergebnis enthalten sind</param>
        /// <param name="loadProperties">Gibt an ob die Eigenschaften hinzugefügt werden sollen</param>
        /// <param name="nameFilter">Namensfilter</param>
        /// <param name="labelFilter">Label-filter</param>
        /// <param name="propertyFilter">Eigenschaft Filter (Falls kein Wertefilter vorhanden werden nur die Knoten genommen, welche die Eigenschaft besitzen)</param>
        /// <param name="propertyValueFilter">Eigenschaftswert Filter: Nur Knoten mit dem angegebenen Wert werden zurückgegeben</param>
        /// <param name="searchDepth">wie weit Verbundene Knoten gesucht werden sollen (hopping)</param>
        /// <returns></returns>
        List<Neo4JNodeDto> GetNodes(int skip, int? limit, out long total, bool loadProperties = true,
            FilterDto nameFilter = null, List<string> labelFilter = null, List<PropertyFilterDto> propertyFilter = null, int searchDepth = 0);

        /// <summary>
        /// Gibt die Knoten sowie alle in diesem Subnetz vorhandenen Relationen zurück.
        /// </summary>
        /// <param name="limit">Wie viele Knoten zurückgegeben werden sollen</param>
        /// <param name="searchDepth">Wie weit verbundene Knoten gesucht werden sollen (hopping)</param>
        /// <param name="loadProperties">Gibt an ob die Eigenschaften hinzugefügt werden sollen</param>
        /// <param name="nameFilter">Namensfilter</param>
        /// <param name="labelFilter">Label-filter</param>
        /// <param name="propertyFilter">Eigenschaft Filter (Falls kein Wertefilter vorhanden werden nur die Knoten genommen, welche die Eigenschaft besitzen)</param>
        /// <param name="propertyValueFilter">Eigenschaftswert Filter: Nur Knoten mit dem angegebenen Wert werden zurückgegeben</param>
        /// <param name="area">Der Abschnitt in welchem gesucht werden soll</param>
        /// <returns>Gefilterte Knoten und zugehörige Relationen</returns>
        Neo4JGraphDto GetNodesWithRelations(int? limit, int searchDepth, bool loadProperties = true,
            FilterDto nameFilter = null, FilterDto labelFilter = null, FilterDto propertyFilter = null,
            FilterDto propertyValueFilter = null);


        /// <summary>
        /// Gibt die Knoten sowie alle in diesem Subnetz vorhandenen Relationen zurück.
        /// </summary>
        /// <param name="limit">Wie viele Knoten zurückgegeben werden sollen</param>
        /// <param name="searchDepth">Wie weit verbundene Knoten gesucht werden sollen (hopping)</param>
        /// <param name="loadProperties">Gibt an ob die Eigenschaften hinzugefügt werden sollen</param>
        /// <param name="nameFilter">Namensfilter</param>
        /// <param name="labelFilter">Label-filter</param>
        /// <param name="propertyFilter">Eigenschaft Filter (Falls kein Wertefilter vorhanden werden nur die Knoten genommen, welche die Eigenschaft besitzen)</param>
        /// <param name="propertyValueFilter">Eigenschaftswert Filter: Nur Knoten mit dem angegebenen Wert werden zurückgegeben</param>
        /// <param name="area">Der Abschnitt in welchem gesucht werden soll</param>
        /// <returns>Gefilterte Knoten und zugehörige Relationen</returns>
        Neo4JGraphDto GetNodesWithRelationsAPOC(int? limit, int searchDepth, bool loadProperties = true,
            FilterDto nameFilter = null, FilterDto labelFilter = null, FilterDto propertyFilter = null,
            FilterDto propertyValueFilter = null);

        /// <summary>
        /// Gibt die Knoten sowie alle in diesem Subnetz vorhandenen Relationen zurück.
        /// </summary>
        /// <param name="limit">Wie viele Knoten zurückgegeben werden sollen</param>
        /// <param name="searchDepth">Wie weit verbundene Knoten gesucht werden sollen (hopping)</param>
        /// <param name="loadProperties">Gibt an ob die Eigenschaften hinzugefügt werden sollen</param>
        /// <param name="nameFilter">Namensfilter</param>
        /// <param name="startLabelFilter">Label-filter</param>
        /// <param name="endLabelFilter">Label-filter</param>
        /// <param name="propertyFilter">Eigenschaft Filter (Falls kein Wertefilter vorhanden werden nur die Knoten genommen, welche die Eigenschaft besitzen)</param>
        /// <param name="propertyValueFilter">Eigenschaftswert Filter: Nur Knoten mit dem angegebenen Wert werden zurückgegeben</param>
        /// <param name="area">Der Abschnitt in welchem gesucht werden soll</param>
        /// <returns>Gefilterte Knoten und zugehörige Relationen</returns>
        Neo4JGraphDto GetNodesWithRelationsDirected(int? limit, int searchDepth, bool loadProperties = true,
            FilterDto nameFilter = null, FilterDto startLabelFilter = null, FilterDto endLabelFilter = null, FilterDto propertyFilter = null,
            FilterDto propertyValueFilter = null);



        /// <summary>
        /// Gibt alle Knoten zurück
        /// </summary>
        /// <param name="loadProperties">Gibt an ob die EIgenschaften hinzugefügt werden sollen</param>
        /// <returns></returns>
        List<Neo4JNodeDto> GetNodes(bool loadProperties = true);
        /// <summary>
        /// Gibt den zugehörigen Knoten zurück
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Neo4JNodeDto GetNode(long id);
        /// <summary>
        /// Gibt den Knoten mit dem angegebnenen Namen zurück
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Neo4JNodeDto GetNodeByName(string name);

        /// <summary>
        /// Gibt die zugehörigen Knoten der Relation zurück
        /// </summary>
        /// <param name="relationId"></param>
        /// <param name="nodeStart"></param>
        /// <param name="nodeEnd"></param>
        void GetConnectedNodes(long relationId, out Neo4JNodeDto nodeStart, out Neo4JNodeDto nodeEnd);

        /// <summary>
        /// Gibt die über eine bestimmte maximallänge verbundenen Knoten zurück
        /// </summary>
        /// <param name="requestSearchDepth"></param>
        /// <param name="nodeIds">Knoten von denen aus die Verbindungen gesucht werden</param>
        /// <returns></returns>
        List<Neo4JNodeDto> GetConnectedNodes(int requestSearchDepth, List<long> nodeIds);

        #endregion
        /// <summary>
        /// Knoten hinzufügen 
        /// </summary>
        /// <param name="node">Knoten</param>
        /// <param name="addProperties">Gibt an ob die Eigenschaften hinzugefügt werden sollen</param>
        /// <param name="importType">Art wie mit doppelten Daten umgegangen werden soll</param>
        /// <returns>Die Id des neuen Knotens oder -1 bei Nichteinfügen</returns>
        long AddNode(Neo4JNodeDto node, bool addProperties = false, ImportType importType = ImportType.Ignore, string layer = null);
        /// <summary>
        /// Liste an Knoten hinzufügen
        /// </summary>
        /// <param name="nodes">Knotenliste</param>
        /// <param name="addProperties">Gibt an ob die Eigenschaften hinzugefügt werden sollen</param>
        /// <param name="importType">Art wie mit doppelten Daten umgegangen werden soll</param>
        void AddNodes(IEnumerable<Neo4JNodeDto> nodes, bool addProperties = false, ImportType importType = ImportType.Ignore);
        /// <summary>
        /// Editiert den Knoten
        /// </summary>
        /// <param name="node"></param>
        /// <param name="ignoreLabels">falls true werden die Labels nicht neu gesetzt</param>
        void EditNode(Neo4JNodeDto node, bool ignoreLabels = false);
        /// <summary>
        /// Löscht den Knoten mit der entsprechenden Id
        /// </summary>
        /// <param name="id"></param>
        void DeleteNode(long id);
        /// <summary>
        /// Schaltet einen mit der entsprechenden Id unsichtbar
        /// </summary>
        /// <param name="id"></param>
        void HideNode(long id, string view);
        /// <summary>
        /// Schaltet einen mit der entsprechenden Id sichtbar
        /// </summary>
        /// <param name="id"></param>
        void ShowNode(long id, string view);
        /// <summary>
        /// Fuegt alle Knoten im Bulk hinzu
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns>neu hinzugefuegten Knoten (mit Id)</returns>
        List<Neo4JNodeDto> AddNodesBulk(List<Neo4JNodeDto> nodes);

        /// <summary>
        /// Mergt alle Knoten im Bulk.
        /// </summary>
        /// <param name="nodes"></param>
        void MergeNodesBulk(List<Neo4JNodeDto> nodes);

        #endregion

        #region Neo4J Node Properties

        /// <summary>
        /// Gibt alle existierenden Eigenschaften der Neo4J Knoten zurück
        /// welche dynamisch hinzugefügt wurden (z.B. nicht Id, Name, X...)
        /// </summary>
        /// <returns></returns>
        List<string> GetAllDynamicPropertyNames();

        /// <summary>
        ///  Gibt zurueck ob das Hinzufuegen einer Neuen Knoteneigenschaft funktioniert hat
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        bool AddNodeProperty(Neo4jNodePropertyDto property);
        /// <summary>
        /// Fügt in einem Schub alle angegebenen Eigenschaften hinzu
        /// </summary>
        /// <param name="networkComponentPropertyDtos"></param>
        void AddComponentPropertiesBulk(List<Neo4jNodePropertyDto> networkComponentPropertyDtos);
        /// <summary>
        /// Gibt zurueck ob das Editieren der Eigenschenschaft funktioniert hat 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        bool EditNodeProperty(Neo4jNodePropertyDto node, string newName = "");
        /// <summary>
        /// Gibt zurueck ob das Loeschen erfolgreich war
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        bool DeleteNodeProperty(Neo4jNodePropertyDto property);

        /// <summary>
        /// Sucht alle verwendeten Eigenschaften aller Knoten (distinct)
        /// </summary>
        /// <returns></returns>
        List<string> GetAllPropertyNames();

        /// <summary>
        /// Gibt alle Values der uebergebenen Eigenschaft zurueck
        /// Alle Knoten werden dafuer durchsucht
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        List<string> GetAvailableNodePropertyValues(string property);

        #endregion

        #region Neo4J Relation Properties

        /// <summary>
        /// Fuegt die angegebene Relationeigenschaft hinzu
        /// Vorsicht, falls bereits vorhanden, wird diese ueberschrieben
        /// </summary>
        /// <param name="property">Gibt zurueck, ob </param>
        /// <returns></returns>
        bool AddRelationProperty(Neo4JRelationPropertyDto property);

        /// <summary>
        /// Editiert die angegebene Relationeigenschaft
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        bool EditRelationProperty(Neo4JRelationPropertyDto property, string newName = "");

        /// <summary>
        /// Loescht die angegebene Relationeigenschaft
        /// </summary>
        /// <param name="property">gibt bei erfolgreicher Ausfuehrung true zurueck</param>
        bool DeleteRelationProperty(Neo4JRelationPropertyDto property);

        /// <summary>
        /// Gibt alle Eiegenschaften von Verbindungen zurück
        /// </summary>
        /// <returns></returns>
        List<string> GetAllRelationPropertyNames(string relationType = "");

        /// <summary>
        /// Gibt alle Eigenschaften von allen Verbindungen zurück (schliesst Eigenschaften wie Id etc aus)
        /// </summary>
        /// <param name="relationtype"></param>
        /// <returns></returns>
        List<string> GetAllDynamicRelationPropertyNames(string relationtype = "");




        #endregion

        #region Neo4J Relation

        /// <summary>
        /// Fuegt alle Verbindungen aufeinmal hinzu
        /// Die dynamischen Eigenschaften werden allerdings nicht hinzugefuegt
        /// </summary>
        /// <param name="relations"></param>
        /// <param name="directed">gibt an ob die Verbindung gerichtet sein soll</param>
        /// <returns>Gibt die Neo4JRelations mit der neuen Id zurueck</returns>
        List<Neo4JRelationDto> AddRelationsBulk(List<Neo4JRelationDto> relations, bool directed = false);

        /// <summary>
        /// Fuegt alle Verbindungen aufeinmal hinzu
        /// Die dynamischen Eigenschaften werden allerdings nicht hinzugefuegt
        /// </summary>
        /// <param name="relations"></param>
        /// <param name="directed">gibt an ob die Verbindung gerichtet sein soll</param>
        /// <returns>Gibt die Neo4JRelations mit der neuen Id zurueck</returns>
        List<Neo4JRelationDto> MergeRelationsBulk(List<Neo4JRelationDto> relations, bool directed = false);

        /// <summary>
        /// Neo4JRelation hinzufügen und gibt die Id zurück (-1 falls nicht hinzugefügt wurde)
        /// </summary>
        /// <param name="neo4Relation"></param>
        /// <param name="importType">Handling für bereits vorhandene Einträge</param>
        /// <param name="directed">Gibt an ob die Kante gerichtet ist oder nicht</param>
        long AddRelation(Neo4JRelationDto neo4Relation, ImportType importType = ImportType.Ignore, bool directed = false);
        /// <summary>
        /// Neo4JRelation editieren
        /// </summary>
        /// <param name="neo4Relation"></param>
        void EditRelation(Neo4JRelationDto neo4Relation);

        /// <summary>
        /// Gibt alles Verbindungen zurück
        /// </summary>
        /// <returns></returns>
        List<Neo4JRelationDto> GetAllRelations(string type = null);
        /// <summary>
        /// Gibt alles Verbindungen für ein Element zurück
        /// </summary>
        /// <returns></returns>
        List<Neo4JRelationDto> GetAllRelations(long id, string type = null);

        /// <summary>
        /// Gibt falls vorhanden die Verbindung zurueck
        /// </summary>
        /// <returns></returns>
        Neo4JRelationDto GetRelation(long relationId);

        /// <summary>
        /// Gibt alle RelationDtos zurück die mit dem Knoten verbunden sind
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        List<Neo4JRelationDto> GetRelations(long nodeId);

        /// <summary>
        /// Löscht die Verbindung zwischen den zwei Knoten
        /// </summary>
        void DeleteRelation(Neo4JRelationDto neo4Relation);

        void DeleteRelation(long relationId);

        /// <summary>
        /// Löscht alle Verbindungen des angegebenen Verbindungstypen
        /// </summary>
        /// <param name="relationType"></param>
        void DeleteRelations(string relationType);

        /// <summary>
        /// Kopiert alle Verbindungen vom ersten Typ und erstellt diese mit dem neuen Typ
        /// </summary>
        /// <param name="sourceRelationType"></param>
        /// <param name="newRelationType"></param>
        /// <param name="copyProperties"></param>
        void CopyRelations(string sourceRelationType, string newRelationType);

        ///// <summary>
        ///// Löscht die Verbindung zwischen den zwei Knoten
        ///// </summary>
        ///// <param name="nodeId1"></param>
        ///// <param name="component2"></param>
        ///// <param name="relationType">Falls nicht angegeben wird der Default Relationtyp verwendet</param>
        //void DeleteRelation(long nodeId1, long component2, string relationType = "");

        /// <summary>
        /// Sucht alle Pfade zwischen den zwei angegebenen Knoten raus
        /// </summary>
        /// <param name="node1Id"></param>
        /// <param name="node2Id"></param>
        /// <param name="maxLengthPath">Maximale Länge eines Pfades (notwendig, da die Suche sonst zu lange dauert)</param>
        /// <param name="relationType"></param>
        /// <returns></returns>
        List<Neo4JRelationPath> GetRelationPathsBetweenNodes(long node1Id, long node2Id, int maxLengthPath, string relationType = null);

        #endregion

        #region Stuff

        /// <summary>
        /// Gibt alle Neo4J Labels zurück
        /// </summary>
        /// <returns></returns>
        List<string> GetAllNodeLabels();

        /// <summary>
        /// Gib alle Neo4J Relation Types zurück
        /// </summary>
        /// <returns></returns>
        List<string> GetAllRelationTypes();
        /// <summary>
        /// Fragt ab ob die Verbindung zu Neo4J steht
        /// </summary>
        /// <returns></returns>
        bool IsConnected();

        /// <summary>
        /// Aendert das label bei allen Knoten
        /// </summary>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        void ChangeLabelName(string oldName, string newName);
        #endregion
        */
    }
}