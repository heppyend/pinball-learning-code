using System;
using System.Collections.Generic;
using QL.Core;

namespace QL.Protocol
{
    /// <summary>
    /// QL API: client.sms.send.phone.code
    /// </summary>
    public class ClientSmsSendPhoneCodeRequest : BaseQLRequest<QL.Protocol.ClientSmsSendPhoneCodeResponse>
    {
        /// <summary>
        /// 区服Id
        /// </summary>
        public long ZoneId { get; set; }

        /// <summary>
        /// 11位手机号码
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// 短信渠道 1岱亿短信渠道 2创瑞云 3阿里大鱼短信渠道 4合肥短信渠道
        /// </summary>
        public long SmsChannel { get; set; }

        #region IQLRequest Members

        public override string GetApiName()
        {
            return "client.sms.send.phone.code";
        }

        public override IDictionary<string, string> GetParameters()
        {
            QLDictionary parameters = new QLDictionary();
            parameters.Add("zone_id", this.ZoneId);
            parameters.Add("phone", this.Phone);
            parameters.Add("sms_channel", this.SmsChannel);
            return parameters;
        }

        public override void Validate()
        {
            QLRequestValidator.ValidateRequired("phone", this.Phone);
            QLRequestValidator.ValidateMaxLength("phone", this.Phone, 11);
        }
        #endregion
    }
}
