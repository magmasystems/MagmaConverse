using System;
using System.Net;
using System.Runtime.Serialization;

namespace Magmasystems.Framework
{
    [Serializable]
    [DataContract]
    public enum ResponseStatusCodes
    {
        [DataMember] OK = HttpStatusCode.OK,
        [DataMember] Error = HttpStatusCode.BadRequest,
    }
}
