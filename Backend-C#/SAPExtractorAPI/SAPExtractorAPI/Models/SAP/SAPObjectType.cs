using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace SAPExtractorAPI.Models.SAP
{
    [DataContract]
    public class SAPObjectType
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int count { get; set; }
    }
}