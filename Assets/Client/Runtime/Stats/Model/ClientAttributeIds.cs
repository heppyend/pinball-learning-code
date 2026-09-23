namespace Pinball.Client.Stats
{
    /// <summary>
    /// 属性 id —— **由权威服务器定义**。客户端只持有常量方便引用，**不在这里规定数值或公式**。
    ///
    /// <para>⚠️ 刻意用 <c>int</c> 常量而不是 <c>enum</c>：加一条属性只需服务器下发新 id + 展示表加一行，
    /// **客户端零代码改动**；用 enum 会迫使每加一条属性就改代码并重编。</para>
    ///
    /// <para>⚠️ 当前这些编号是**本地模拟占位**（服务端契约尚未提供），接入服务器后按服务器定义替换。
    /// **不要**把它们当成既定协议。</para>
    /// </summary>
    public static class ClientAttributeIds
    {
        // ---- 以下为占位 id，等服务器契约确认后替换（见 Stats/MODULE.md「待确认」）----
        public const int Hp = 1;
        public const int Attack = 2;
        public const int Defence = 3;
        public const int Speed = 4;
        public const int CritRate = 5;
        public const int CritHurt = 6;
        public const int CombatPower = 7;
        public const int Energy = 8;
    }
}
