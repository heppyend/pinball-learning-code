using System;
using System.Collections.Generic;
using QL.Core;

namespace QL.Protocol
{
    /// <summary>
    /// QL API: client.account.check.exist
    /// </summary>
    public class ClientAccountCheckExistRequest : BaseQLRequest<QL.Protocol.ClientAccountCheckExistResponse>
    {
        /// <summary>
        /// 区服Id
        /// </summary>
        public long ZoneId { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        public string Phone { get; set; }

        #region IQLRequest Members

        public override string GetApiName()
        {
            return "client.account.check.exist";
        }

        public override IDictionary<string, string> GetParameters()
        {
            QLDictionary parameters = new QLDictionary();
            parameters.Add("zone_id", this.ZoneId);
            parameters.Add("phone", this.Phone);
            return parameters;
        }

        public override void Validate()
        {
            QLRequestValidator.ValidateRequired("phone", this.Phone);
            QLRequestValidator.ValidateMaxLength("phone", this.Phone, 16);
        }
        #endregion
    }
}
