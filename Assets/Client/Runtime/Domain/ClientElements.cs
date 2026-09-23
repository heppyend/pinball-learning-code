namespace Pinball.Client.Domain
{
    /// <summary>
    /// 元素映射 —— 配置 `THero.Element` 是 **int 1..6**（**不是** `"Fire"` / `"Water"` 这类字符串），
    /// 而 `FormationPage编队` 的筛选区正好是 **7 个按钮 = `全` + 6 个元素**。
    ///
    /// **顺序由负责人 2026-09-20 确认：按筛选按钮的排列，`1..6` = 光 / 水 / 土 / 风 / 火 / 暗。**
    /// 这是"从数据与界面共同推导并确认"的结果，不是实现者的猜测。
    /// </summary>
    public static class ClientElements
    {
        /// <summary>元素 id 下限。</summary>
        public const int Min = 1;

        /// <summary>元素 id 上限。</summary>
        public const int Max = 6;

        /// <summary>`全`（不筛选）的显示名。</summary>
        public const string AllName = "全";

        /// <summary>不筛选时使用的 id。</summary>
        public const int None = 0;

        /// <summary>按筛选按钮排列顺序：1..6 = 光 / 水 / 土 / 风 / 火 / 暗。</summary>
        private static readonly string[] Names = { "光", "水", "土", "风", "火", "暗" };

        /// <summary>元素 id → 中文名；`全`/越界返回空串。</summary>
        public static string Name(int element)
        {
            if (element < Min || element > Max)
                return string.Empty;
            return Names[element - Min];
        }

        /// <summary>中文名 → 元素 id；`全` 或无法识别返回 <see cref="None"/>（0 = 不筛选）。</summary>
        public static int FromName(string name)
        {
            if (string.IsNullOrEmpty(name) || name == AllName)
                return None;

            for (int index = 0; index < Names.Length; index++)
            {
                if (Names[index] == name)
                    return index + Min;
            }
            return None;
        }

        /// <summary>全部元素 id（1..6），供筛选按钮批量使用。</summary>
        public static int[] AllIds()
        {
            int[] ids = new int[Max - Min + 1];
            for (int index = 0; index < ids.Length; index++)
                ids[index] = Min + index;
            return ids;
        }
    }
}
