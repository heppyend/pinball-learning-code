using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using QL.Core;

namespace QL.Protocol
{
    /// <summary>
    /// QL API: client.account.check.exist
    /// </summary>
    public class ClientAccountCheckExistResponse : QLResponse
    {
        /// <summary>
        /// 是否存在 1是0否
        /// </summary>
        [XmlElement("is_exist")]
        public long IsExist { get; set; }

    }
}
