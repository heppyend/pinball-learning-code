namespace Pinball.Client.Stats
{
    /// <summary>
    /// 技能 / 天赋的**玩家状态** —— 字段全部是**权威服务器下发的数值**，客户端不做任何推导。
    ///
    /// <para>用 <c>struct</c> 便于零分配地按值传递与存放。</para>
    /// </summary>
    public struct ClientAbilityState
    {
        /// <summary>技能 id 或 天赋 id。</summary>
        public int AbilityId;

        /// <summary>等级；`0` = 尚未获得。</summary>
        public int Level;

        /// <summary>是否已解锁。</summary>
        public bool Unlocked;

        /// <summary>剩余冷却回合；`0` = 就绪。**由服务器计算**。</summary>
        public int CooldownRemaining;

        /// <summary>经验（天赋用；无此玩法的技能恒 0）。**由服务器计算**。</summary>
        public int Exp;

        /// <summary>是否可升级 —— **由服务器判定**（客户端不检查材料/货币够不够）。</summary>
        public bool CanUpgrade;

        /// <summary>是否已拥有（等级 &gt; 0 或已解锁）。</summary>
        public bool IsOwned
        {
            get { return Level > 0 || Unlocked; }
        }
    }
}
