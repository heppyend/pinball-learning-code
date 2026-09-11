using System;
using System.Collections.Generic;
using QL.Core;

namespace QL.Protocol
{
    /// <summary>
    /// QL API: client.channel.login.get.app.info
    /// </summary>
    public class ClientChannelLoginGetAppInfoRequest : BaseQLRequest<QL.Protocol.ClientChannelLoginGetAppInfoResponse>
    {
        /// <summary>
        /// 渠道Id,1:Oppo,2:华为
        /// </summary>
        public long ChannelId { get; set; }

        /// <summary>
        /// 区服Id
        /// </summary>
        public long ZoneId { get; set; }

        #region IQLRequest Members

        public override string GetApiName()
        {
            return "client.channel.login.get.app.info";
        }

        public override IDictionary<string, string> GetParameters()
        {
            QLDictionary parameters = new QLDictionary();
            parameters.Add("channel_id", this.ChannelId);
            parameters.Add("zone_id", this.ZoneId);
            return parameters;
        }

        public override void Validate()
        {
        }
        #endregion
    }
}
