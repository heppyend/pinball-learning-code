using LC.Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

public class THotUpdate
{
    /// <summary>
    /// 唯一索引
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 游戏Id（1001不能动，默认大厅）
    /// </summary>
    public int GameId { get; set; }

    /// <summary>
    /// 游戏名(中文)
    /// </summary>
    public string NameCN { get; set; }

    /// <summary>
    /// 游戏名(英文)
    /// </summary>
    public string NameEN { get; set; }

    /// <summary>
    /// 版本号
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// 热更包下载链接
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// 是否开启(1开启0关闭,大厅默认开启不可关闭)
    /// </summary>
    public int OpenFlag { get; set; }

    /// <summary>
    /// 服务组Id
    /// </summary>
    public int GameGroupType { get; set; }

    /// <summary>
    /// 屏幕方向(1横屏2竖屏)
    /// </summary>
    public int ScreenOrientation { get; set; }

    /// <summary>
    /// 自动安装(0不1是)
    /// </summary>
    public int AutoUpdate { get; set; }

    /// <summary>
    /// 自动安装(0不1是)
    /// </summary>
    public int GamePlayId { get; set; }
}

public static class THotUpdateHelper
{
    public static readonly string TableName = "HotUpdate";
    public static readonly Type TableType = typeof(THotUpdate);

    public static Dictionary<int, THotUpdate> DataMap;

    public static void LoadData(List<object> rows)
    {
        DataMap = new Dictionary<int, THotUpdate>();
        foreach (var t in rows.Cast<THotUpdate>())
            DataMap[t.Id] = t;
    }

    public static THotUpdate GetRow(int id)
    {
        THotUpdate r = null;
        return DataMap.TryGetValue(id, out r) ? r : null;
    }
}
