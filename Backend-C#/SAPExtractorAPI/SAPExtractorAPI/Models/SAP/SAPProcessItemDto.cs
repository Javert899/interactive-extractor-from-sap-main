using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace SAPExtractorAPI.Models.SAP
{
    [DataContract]
    public class SAPProcessItemDto
    {
        [DataMember]
        public string Mandant { get; set; }

        [DataMember]
        public string Objectclass { get; set; }

        [DataMember]
        public string ObjectId { get; set; }

        [DataMember]
        public string ChangeNr { get; set; }
    }
}