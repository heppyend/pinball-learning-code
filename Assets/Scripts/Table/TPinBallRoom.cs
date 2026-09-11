using LC.Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

public class TPinBallRoom
{
    /// <summary>
    /// 唯一索引
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 房间名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 房间类型
    /// </summary>
    public int Type { get; set; }

    /// <summary>
    /// 刷新怪物
    /// </summary>
    public JArray MosterAppear { get; set; }

    /// <summary>
    /// 获胜奖励
    /// </summary>
    public JArray Rewards { get; set; }
}

public static class TPinBallRoomHelper
{
    public static readonly string TableName = "PinBallRoom";
    public static readonly Type TableType = typeof(TPinBallRoom);

    public static Dictionary<int, TPinBallRoom> DataMap;

    public static void LoadData(List<object> rows)
    {
        DataMap = new Dictionary<int, TPinBallRoom>();
        foreach (var t in rows.Cast<TPinBallRoom>())
            DataMap[t.Id] = t;
    }

    public static TPinBallRoom GetRow(int id)
    {
        TPinBallRoom r = null;
        return DataMap.TryGetValue(id, out r) ? r : null;
    }
}
