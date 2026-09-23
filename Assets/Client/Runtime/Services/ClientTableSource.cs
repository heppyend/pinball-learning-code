using System;
using System.Collections.Generic;
using System.IO;
using Unity.IO.Compression;   // 与工程既有实现一致（TableLoadHelper 也用这个），避免 WebGL 下多一个未知
using System.Text;
using UnityEngine;

namespace Pinball.Client.Services
{
    /// <summary>
    /// 客户端静态配置的**表读取入口**（只读）。
    ///
    /// <para><b>数据源优先级与游戏一致</b>（"两者"：本地进包兜底 + 远端热更落盘）：</para>
    /// <list type="number">
    /// <item><c>{Application.persistentDataPath}/config.pkg</c> —— 启动流程 `TableLoadHelper.LoadFromNet`
    /// 下载后落盘的远端表包；存在就优先用它。</item>
    /// <item><c>Resources/Table/&lt;表名&gt;.json</c> —— 随包发布的本地表（没有远端包时的兜底）。</item>
    /// </list>
    ///
    /// <para><b>为什么不直接用 `TableLoadHelper`：</b></para>
    /// <list type="number">
    /// <item>它在 **`HotFix` 程序集**，`Assets/Client`（`Assembly-CSharp`）直接引用会复发 IL1005
    /// （见 `TCP_WEBGL_HANDOFF.md`）；走反射又会读到上次构建的旧成员。</item>
    /// <item>它按 `TableLoadConfig` 的 **21 张清单整批加载**，连客户端用不到的表（如 `Skill`）也一并解析 ——
    /// 而 `Skill` 表与工程里的旧生成类结构不符，会刷 **99 条** `配置表解析出错：System.FormatException`，
    /// 把 Console 淹掉、挡住客户端验收（见 `BUG_TRACKER.md` BUG-022）。
    /// 客户端只需要自己那几张，**只读自己需要的**即可从根上避免。</item>
    /// </list>
    ///
    /// <para><b>压缩库</b>：用工程既有的 <c>Unity.IO.Compression</c>（`Assets/Unity.IO.Compression/`，其 asmdef 默认 autoReferenced）
    /// —— 与 <c>TableLoadHelper</c> 同源，避免 WebGL / 微信小游戏下引入 `System.IO.Compression` 这个未验证的依赖。</para>
    ///
    /// <para><b>远端包格式</b>（与 `TableLoadHelper._parsePackageContent` 保持一致的只读复刻）：</para>
    /// <code>
    /// GZip 压缩；解压后： [version:1 byte]  然后反复 [tableName:utf8\0][tableContent:utf8\0]
    /// </code>
    /// 只支持 <c>version == 1</c>；解析失败或不支持的版本会**回退到本地 json** 并警告一次。
    /// ⚠️ 若上游改了包格式，需同步这里（权威实现：`Assets/Scripts/Table/TableLoadHelper.cs`）。
    /// </summary>
    public static class ClientTableSource
    {
        private const string LocalJsonDirectory = "Table/";
        private const string LocalPackageFileName = "config.pkg";
        private const byte SupportedPackageVersion = 1;

        private static Dictionary<string, string> _packagedTables;
        private static bool _packageResolved;
        private static bool _packageWarned;

        /// <summary>包内该表的内容；没有远端包时为 null。</summary>
        private static string GetPackagedContent(string tableName)
        {
            if (!_packageResolved)
            {
                _packageResolved = true;
                _packagedTables = TryReadPackage();
            }

            if (_packagedTables == null)
                return null;

            string content;
            return _packagedTables.TryGetValue(tableName, out content) ? content : null;
        }

        private static Dictionary<string, string> TryReadPackage()
        {
            string path = Path.Combine(Application.persistentDataPath, LocalPackageFileName);
            if (!File.Exists(path))
                return null;

            try
            {
                byte[] raw = File.ReadAllBytes(path);
                byte[] plain = Gunzip(raw);

                int offset = 0;
                byte version = plain[offset++];
                if (version != SupportedPackageVersion)
                {
                    WarnOnce("远端表包版本不支持：" + version + "（本端只认 " + SupportedPackageVersion + "），已回退本地表。");
                    return null;
                }

                Dictionary<string, string> tables = new Dictionary<string, string>();
                while (offset < plain.Length)
                {
                    string name = ReadUtf8String(plain, ref offset);
                    string content = ReadUtf8String(plain, ref offset);
                    if (!string.IsNullOrEmpty(name))
                        tables[name] = content;
                }

                return tables;
            }
            catch (Exception ex)
            {
                WarnOnce("远端表包解析失败，已回退本地表：" + ex.Message);
                return null;
            }
        }

        private static byte[] Gunzip(byte[] content)
        {
            using (MemoryStream input = new MemoryStream(content))
            using (GZipStream zip = new GZipStream(input, CompressionMode.Decompress))
            using (MemoryStream output = new MemoryStream())
            {
                byte[] buffer = new byte[4096];
                int read;
                while ((read = zip.Read(buffer, 0, buffer.Length)) > 0)
                    output.Write(buffer, 0, read);
                return output.ToArray();
            }
        }

        /// <summary>读以 0 结尾的 UTF-8 字符串，并把 <paramref name="offset"/> 前移。</summary>
        private static string ReadUtf8String(byte[] data, ref int offset)
        {
            int start = offset;
            while (offset < data.Length && data[offset] != 0)
                offset++;

            string value = Encoding.UTF8.GetString(data, start, offset - start);
            if (offset < data.Length)
                offset++;   // 跳过结尾的 0
            return value;
        }

        private static void WarnOnce(string message)
        {
            if (_packageWarned)
                return;
            _packageWarned = true;
            Debug.LogWarning("[Client] " + message);
        }

        /// <summary>
        /// 取一张表的原始 json 文本（远端包优先，其次本地 `Resources/Table`）。
        /// 找不到返回 false —— 调用方应显示空态而不是崩溃。
        /// </summary>
        public static bool TryGetJson(string tableName, out string json)
        {
            json = null;

            string packaged = GetPackagedContent(tableName);
            if (!string.IsNullOrEmpty(packaged))
            {
                json = packaged;
                return true;
            }

            TextAsset local = Resources.Load<TextAsset>(LocalJsonDirectory + tableName);
            if (local == null)
                return false;

            json = local.text;
            return true;
        }

        /// <summary>清空缓存（退出 Play / 域重载 / 远端包更新后重新解析）。</summary>
        public static void Reset()
        {
            _packagedTables = null;
            _packageResolved = false;
            _packageWarned = false;
        }

        /// <summary>
        /// 当前是否在用**落盘的远端表包**（`config.pkg`）。`false` = 用本地 `Resources/Table/*.json`。
        /// 供诊断打印 —— 排查"表为什么是旧的"时，这一项是关键（见 BUG_TRACKER.md BUG-021）。
        /// </summary>
        public static bool UsingPackagedTables
        {
            get
            {
                if (!_packageResolved)
                {
                    _packageResolved = true;
                    _packagedTables = TryReadPackage();
                }
                return _packagedTables != null && _packagedTables.Count > 0;
            }
        }

        /// <summary>远端包内的表数量（未使用远端包时为 0）。</summary>
        public static int PackagedTableCount
        {
            get { return UsingPackagedTables ? _packagedTables.Count : 0; }
        }

        /// <summary>数据源的可读描述，用于日志。</summary>
        public static string DescribeSource()
        {
            return UsingPackagedTables
                ? "远端落盘包 config.pkg（" + PackagedTableCount + " 张表）"
                : "本地 Resources/Table（未发现可用的 config.pkg）";
        }
    }
}
