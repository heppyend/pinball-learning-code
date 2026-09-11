using System;
using System.Collections.Generic;
using QL.Core;

namespace QL.Protocol
{
    /// <summary>
    /// QL API: client.account.login
    /// </summary>
    public class ClientAccountLoginRequest : BaseQLRequest<QL.Protocol.ClientAccountLoginResponse>
    {
        /// <summary>
        /// 区服Id
        /// </summary>
        public long ZoneId { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// 登录密码，使用CA3加密，固定秘钥：19357
        /// </summary>
        public string Password { get; set; }

        #region IQLRequest Members

        public override string GetApiName()
        {
            return "client.account.login";
        }

        public override IDictionary<string, string> GetParameters()
        {
            QLDictionary parameters = new QLDictionary();
            parameters.Add("zone_id", this.ZoneId);
            parameters.Add("phone", this.Phone);
            parameters.Add("password", this.Password);
            return parameters;
        }

        public override void Validate()
        {
            QLRequestValidator.ValidateRequired("phone", this.Phone);
            QLRequestValidator.ValidateMaxLength("phone", this.Phone, 16);
            QLRequestValidator.ValidateRequired("password", this.Password);
            QLRequestValidator.ValidateMaxLength("password", this.Password, 64);
        }
        #endregion
    }
}
