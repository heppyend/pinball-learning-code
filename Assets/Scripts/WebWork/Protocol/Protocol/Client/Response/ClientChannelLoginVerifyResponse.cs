using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using QL.Core;

namespace QL.Protocol
{
    /// <summary>
    /// QL API: client.channel.login.verify
    /// </summary>
    public class ClientChannelLoginVerifyResponse : QLResponse
    {
        /// <summary>
        /// 用户唯一标识符
        /// </summary>
        [XmlElement("unique_token")]
        public string UniqueToken { get; set; }

    }
}
