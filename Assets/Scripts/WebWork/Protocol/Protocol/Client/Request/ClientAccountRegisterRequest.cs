using System;
using System.Collections.Generic;
using QL.Core;

namespace QL.Protocol
{
    /// <summary>
    /// QL API: client.account.register
    /// </summary>
    public class ClientAccountRegisterRequest : BaseQLRequest<QL.Protocol.ClientAccountRegisterResponse>
    {
        /// <summary>
        /// 区服Id
        /// </summary>
        public long ZoneId { get; set; }

        /// <summary>
        /// 注册的手机号
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// 注册的密码，使用CA3加密，固定秘钥：19357
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 短信验证的AppKey
        /// </summary>
        public string SmsAppKey { get; set; }

        /// <summary>
        /// 短信验证的区号
        /// </summary>
        public string SmsZone { get; set; }

        /// <summary>
        /// 短信验证的验证码
        /// </summary>
        public string SmsCode { get; set; }

        /// <summary>
        /// 0Mob渠道 1其他渠道
        /// </summary>
        public long SmsChannel { get; set; }

        #region IQLRequest Members

        public override string GetApiName()
        {
            return "client.account.register";
        }

        public override IDictionary<string, string> GetParameters()
        {
            QLDictionary parameters = new QLDictionary();
            parameters.Add("zone_id", this.ZoneId);
            parameters.Add("phone", this.Phone);
            parameters.Add("password", this.Password);
            parameters.Add("sms_app_key", this.SmsAppKey);
            parameters.Add("sms_zone", this.SmsZone);
            parameters.Add("sms_code", this.SmsCode);
            parameters.Add("sms_channel", this.SmsChannel);
            return parameters;
        }

        public override void Validate()
        {
            QLRequestValidator.ValidateRequired("phone", this.Phone);
            QLRequestValidator.ValidateMaxLength("phone", this.Phone, 16);
            QLRequestValidator.ValidateRequired("password", this.Password);
            QLRequestValidator.ValidateMaxLength("password", this.Password, 64);
            QLRequestValidator.ValidateMaxLength("sms_app_key", this.SmsAppKey, 64);
            QLRequestValidator.ValidateMaxLength("sms_zone", this.SmsZone, 10);
            QLRequestValidator.ValidateRequired("sms_code", this.SmsCode);
            QLRequestValidator.ValidateMaxLength("sms_code", this.SmsCode, 10);
        }
        #endregion
    }
}
