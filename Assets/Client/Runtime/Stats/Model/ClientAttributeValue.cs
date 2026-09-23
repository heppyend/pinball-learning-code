namespace Pinball.Client.Stats
{
    /// <summary>
    /// 一条属性：id + **服务器已算好的值**。
    /// 用 <c>struct</c> 是为了零分配地放进 <c>List&lt;ClientAttributeValue&gt;</c>。
    ///
    /// <para>客户端**不推导、不叠加、不套公式**：这里出现的值一律来自 <see cref="IClientStatsGateway"/>。</para>
    /// </summary>
    public struct ClientAttributeValue
    {
        /// <summary>属性 id（服务器定义，见 <see cref="ClientAttributeIds"/>）。</summary>
        public int Id;

        /// <summary>属性值（服务器算好的最终值）。</summary>
        public int Value;

        public ClientAttributeValue(int id, int value)
        {
            Id = id;
            Value = value;
        }
    }
}
