using System;
using System.Collections.Generic;
using QL.Core;

namespace QL.Protocol
{
    /// <summary>
    /// QL API: client.channel.login.verify
    /// </summary>
    public class ClientChannelLoginVerifyRequest : BaseQLRequest<QL.Protocol.ClientChannelLoginVerifyResponse>
    {
        /// <summary>
        /// 渠道Id
        /// </summary>
        public long ChannelId { get; set; }

        /// <summary>
        /// 登录验证所需参数的json串
        /// </summary>
        public string Params { get; set; }

        /// <summary>
        /// 区服Id
        /// </summary>
        public long ZoneId { get; set; }

        #region IQLRequest Members

        public override string GetApiName()
        {
            return "client.channel.login.verify";
        }

        public override IDictionary<string, string> GetParameters()
        {
            QLDictionary parameters = new QLDictionary();
            parameters.Add("channel_id", this.ChannelId);
            parameters.Add("params", this.Params);
            parameters.Add("zone_id", this.ZoneId);
            return parameters;
        }

        public override void Validate()
        {
            QLRequestValidator.ValidateRequired("params", this.Params);
        }
        #endregion
    }
}
