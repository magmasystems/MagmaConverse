using System;
using System.Net;
using System.Runtime.Serialization;

namespace MagmaConverse.Framework
{
    [Serializable]
    [DataContract]
    public enum ResponseStatusCodes
    {
        [DataMember] OK = HttpStatusCode.OK,
        [DataMember] Error = HttpStatusCode.BadRequest,
    }
}
