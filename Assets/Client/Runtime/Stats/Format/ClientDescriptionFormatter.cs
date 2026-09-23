using System.Collections.Generic;
using System.Text;

namespace Pinball.Client.Stats
{
    /// <summary>
    /// <see cref="IClientDescriptionFormatter"/> 的实现 —— **说明文本的唯一拼接点**。
    ///
    /// <para><b>它处理什么</b>：表里的描述把"按等级分档"的数值写在同一句里。实测两种写法都要支持：</para>
    /// <code>
    /// 每段各自带单位（实测就是这个）："回复其8%/9%/10%生命值"    EffectLevel = [1,10,20]
    /// 只有末段带单位：            "造成60%/70%/80%攻击力伤害"
    /// 等级 1  → "回复其8%生命值"     等级 15 → "回复其9%生命值"
    /// </code>
    /// <para>单个数字（"冷却3回合"、"3枚追踪弹"）**不动**。</para>
    ///
    /// <para><b>边界</b>：只选档、只替换文本，**不做任何数值计算**（不乘系数、不叠加加成）。</para>
    ///
    /// <para><b>GC</b>：① <c>(id, level)</c> 结果缓存；② 模板里没有分档片段时**直接返回模板字符串本身**（零分配）；
    /// ③ 解析用 <c>stackalloc</c> 的 <c>Span&lt;int&gt;</c>，不建数组、不建中间字符串；<see cref="StringBuilder"/> 复用。</para>
    ///
    /// <para>⚠️ 已知限制：要求各段后缀一致（实测数据都一致）。若出现"只有中间某段带单位"的写法，
    /// 该片段会退回**原样输出**（不会算错，只是不选档）。</para>
    /// </summary>
    public sealed class ClientDescriptionFormatter : IClientDescriptionFormatter
    {
        /// <summary>一个分档片段最多支持的段数（超出则原样输出）。</summary>
        private const int MaxPartCount = 8;

        private readonly Dictionary<long, string> _cache = new Dictionary<long, string>(128);
        private readonly StringBuilder _buffer = new StringBuilder(256);

        public string Format(int abilityId, string template, int[] effectLevels, int level)
        {
            if (string.IsNullOrEmpty(template))
                return string.Empty;

            long key = MakeKey(abilityId, level);
            string cached;
            if (_cache.TryGetValue(key, out cached))
                return cached;

            bool replaced = TryFormat(abilityId, template, effectLevels, level, _buffer);

            // 没有分档片段 ⇒ 直接复用模板字符串，不产生新字符串
            string result = replaced ? _buffer.ToString() : template;
            _cache[key] = result;
            return result;
        }

        public bool TryFormat(int abilityId, string template, int[] effectLevels, int level, StringBuilder buffer)
        {
            if (buffer == null)
                return false;

            buffer.Length = 0;
            if (string.IsNullOrEmpty(template))
                return false;

            int tier = ResolveTierIndex(effectLevels, level);
            bool replaced = false;
            int index = 0;
            int length = template.Length;

            // 在方法内一次性分配（不能放进循环：stackalloc 要到方法返回才释放）
            System.Span<int> parts = stackalloc int[MaxPartCount];

            while (index < length)
            {
                char current = template[index];
                if (!IsDigit(current))
                {
                    buffer.Append(current);
                    index++;
                    continue;
                }

                int afterDigits = index;
                while (afterDigits < length && IsDigit(template[afterDigits]))
                    afterDigits++;

                int end;
                int partCount = CollectProgression(template, index, afterDigits, parts, out end);

                if (partCount < 2)
                {
                    // 单个数字（或段数超限）：原样抄
                    for (int i = index; i < afterDigits; i++)
                        buffer.Append(template[i]);
                    index = afterDigits;
                    continue;
                }

                int chosenTier = tier < 0 ? 0 : (tier < partCount ? tier : partCount - 1);
                AppendInt(buffer, parts[chosenTier]);
                AppendPartSuffix(buffer, template, afterDigits);   // 每段共用的后缀（如 "%"）

                replaced = true;
                index = end;
            }

            return replaced;
        }

        public void Clear()
        {
            _cache.Clear();
            _buffer.Length = 0;
        }

        // ------------------------------------------------------------------
        // 解析（零分配）
        // ------------------------------------------------------------------

        /// <summary>
        /// 从 <paramref name="numberStart"/> 处解析 `d&lt;后缀&gt;/d&lt;后缀&gt;/…`：
        /// 把各段数值写进 <paramref name="parts"/>，返回段数（&lt;2 表示不是分档），
        /// <paramref name="end"/> 为整个片段之后的位置。
        /// </summary>
        private static int CollectProgression(string text, int numberStart, int afterDigits, System.Span<int> parts, out int end)
        {
            end = afterDigits;

            int firstValue;
            if (!TryReadNumber(text, numberStart, out firstValue))
                return 0;

            parts[0] = firstValue;

            // 第一段的后缀：从数字之后起，到下一个 '/' 或数字为止（实测为 "%"，也可能为空）
            int suffixStart = afterDigits;
            int suffixEnd = suffixStart;
            int length = text.Length;
            while (suffixEnd < length && text[suffixEnd] != '/' && !IsDigit(text[suffixEnd]))
                suffixEnd++;

            int suffixLength = suffixEnd - suffixStart;

            // 后面必须紧跟 "/数字" 才算分档
            if (suffixLength > 0 && suffixEnd >= length)
                return 1;
            if (!IsSeparator(text, suffixEnd))
                return 1;

            int cursor = suffixEnd;
            int count = 1;

            while (count < parts.Length && IsSeparator(text, cursor))
            {
                int partStart = cursor + 1;
                if (partStart >= length || !IsDigit(text[partStart]))
                    break;

                int value;
                if (!TryReadNumber(text, partStart, out value))
                    break;

                parts[count] = value;
                count++;

                // 跳过本段数字
                cursor = partStart;
                while (cursor < length && IsDigit(text[cursor]))
                    cursor++;

                // 每段后缀是否与第一段一致？不一致就到此为止（该段不并入）
                if (suffixLength > 0)
                {
                    if (!MatchesSuffix(text, cursor, suffixStart, suffixLength))
                    {
                        count--;
                        cursor = partStart;
                        while (cursor < length && IsDigit(text[cursor]))
                            cursor++;
                        break;
                    }
                    cursor += suffixLength;
                }
            }

            if (count < 2)
                return 1;

            end = cursor;
            return count;
        }

        private static bool IsSeparator(string text, int index)
        {
            return index + 1 < text.Length && text[index] == '/' && IsDigit(text[index + 1]);
        }

        private static bool MatchesSuffix(string text, int index, int suffixStart, int suffixLength)
        {
            if (index + suffixLength > text.Length)
                return false;

            for (int i = 0; i < suffixLength; i++)
            {
                if (text[index + i] != text[suffixStart + i])
                    return false;
            }
            return true;
        }

        private static bool TryReadNumber(string text, int start, out int value)
        {
            value = 0;
            if (start >= text.Length || !IsDigit(text[start]))
                return false;

            int index = start;
            while (index < text.Length && IsDigit(text[index]))
            {
                value = value * 10 + (text[index] - '0');
                index++;
            }
            return true;
        }

        /// <summary>把第一段之后、下一个 '/' 或数字之前的内容当作**共用后缀**抄进 buffer（如 "%"）。</summary>
        private static void AppendPartSuffix(StringBuilder buffer, string text, int afterDigits)
        {
            int index = afterDigits;
            int length = text.Length;
            while (index < length && text[index] != '/' && !IsDigit(text[index]))
            {
                buffer.Append(text[index]);
                index++;
            }
        }

        private static bool IsDigit(char value)
        {
            return value >= '0' && value <= '9';
        }

        /// <summary>按 `EffectLevel` 选出档位下标：取"门槛 &lt;= 等级"的个数 - 1（未达第 1 档时按第 1 档）。</summary>
        private static int ResolveTierIndex(int[] effectLevels, int level)
        {
            if (effectLevels == null || effectLevels.Length == 0)
                return 0;

            int tier = 0;
            for (int index = 0; index < effectLevels.Length; index++)
            {
                if (level >= effectLevels[index])
                    tier = index;
            }
            return tier;
        }

        private static void AppendInt(StringBuilder buffer, int value)
        {
            if (value == 0)
            {
                buffer.Append('0');
                return;
            }

            if (value < 0)
            {
                buffer.Append('-');
                value = -value;
            }

            // 最多 10 位，栈上反转，不产生字符串（GC 原则）
            int start = buffer.Length;
            while (value > 0)
            {
                buffer.Append((char)('0' + (value % 10)));
                value /= 10;
            }

            int end = buffer.Length - 1;
            while (start < end)
            {
                char temp = buffer[start];
                buffer[start] = buffer[end];
                buffer[end] = temp;
                start++;
                end--;
            }
        }

        private static long MakeKey(int abilityId, int level)
        {
            return ((long)abilityId << 32) | (uint)level;
        }
    }
}
