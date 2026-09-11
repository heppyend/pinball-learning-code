using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using QL.Core;

namespace QL.Protocol
{
    /// <summary>
    /// QL API: client.account.register
    /// </summary>
    public class ClientAccountRegisterResponse : QLResponse
    {
        /// <summary>
        /// 登录时的唯一Token串
        /// </summary>
        [XmlElement("token")]
        public string Token { get; set; }

    }
}
