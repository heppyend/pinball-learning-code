using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using QL.Core;

namespace QL.Protocol
{
    /// <summary>
    /// QL API: client.channel.login.get.app.info
    /// </summary>
    public class ClientChannelLoginGetAppInfoResponse : QLResponse
    {
        /// <summary>
        /// 应用Id
        /// </summary>
        [XmlElement("app_id")]
        public string AppId { get; set; }

        /// <summary>
        /// 应用Key
        /// </summary>
        [XmlElement("app_key")]
        public string AppKey { get; set; }

        /// <summary>
        /// 应用Secret
        /// </summary>
        [XmlElement("app_secret")]
        public string AppSecret { get; set; }

        /// <summary>
        /// 扩展参数
        /// </summary>
        [XmlElement("extend")]
        public string Extend { get; set; }

    }
}
