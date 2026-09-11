using JBPROTO;
using System;
using System.Collections.Generic;
using LC.Newtonsoft.Json.Linq;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 奖励类容器
/// </summary>
public sealed class RewardItem
{
    /// <summary>
    /// 奖励模型
    /// </summary>
    public GameObject Reward;
    /// <summary>
    /// 物体信息列表
    /// </summary>
    public List<ItemInfo> Infos;
    /// <summary>
    /// Buff信息列表
    /// </summary>
    public List<BuffInfo> BuffInfos;
    /// <summary>
    /// 信息列表数目
    /// </summary>
    public int Quantity
    {
        //
        //摘要:
        //    统计信息数目
        get
        {
            return Infos.Count();
        }
    }
    /// <summary>
    /// 信息列表数目
    /// </summary>
    public int Count()
    {
        return Infos.Count;
    }

    public RewardItem (GameObject _Reward, List<ItemInfo> _Infos)
    {
        Reward = _Reward;
        Infos = _Infos ;
    }
    public RewardItem(GameObject _Reward, List<BuffInfo> _Infos)
    {
        Reward = _Reward;
        BuffInfos = _Infos;
    }
}

/// <summary>
/// 进肚条容器类
/// </summary>
public sealed class Progress
{
    private const int _ChilderIndex = 0;
    /// <summary>
    /// 进肚条小穴
    /// </summary>
    public RectTransform Slider;
    /// <summary>
    /// 进肚条定位
    /// </summary>
    public RectTransform RectFill
    {
        get
        {
            return Slider?.GetChild(_ChilderIndex).GetComponent<RectTransform>();
        }
    }
    /// <summary>
    /// 进肚条展示
    /// </summary>
    public Image ImgFill
    {
        get
        {
            return Slider?.GetChild(_ChilderIndex).GetComponent<Image>();
        }
    }
    /// <summary>
    /// 当前进肚条
    /// </summary>
    public Text CurrentProgress
    {
        get
        {
            return Slider?.Find("CurrentProgress")?.GetComponent<Text>() ??null;
        }
    }

    /// <summary>
    /// 最大进肚条
    /// </summary>
    public Text MaxProgress
    {
        get
        {
            return Slider?.Find("MaxProgress")?.GetComponent<Text>() ?? null;
        }
    }

    /// <summary>
    /// 最大进肚条
    /// </summary>
    public Text CenterProgress
    {
        get
        {
            return Slider?.Find("CenterProgress")?.GetComponent<Text>() ?? null;
        }
    }

    /// <summary>
    /// 当前进度值
    /// </summary>
    public long CurrentValue
    {
        get
        {
            return long.Parse(CurrentProgress?.text?? "0");
        }
    }

    /// <summary>
    /// 当前进度值
    /// </summary>
    public long MaxValue
    {
        get
        {
            return long.Parse(MaxProgress?.text ?? "0");
        }
    }

    /// <summary>
    /// 进度设置
    /// </summary>
    // public void Value(ProgressValue value,Action action=null)
    // {
    //     
    //     if (CurrentProgress == null || MaxProgress == null)
    //         return;
    //
    //     bool isFalse = value.Max < 0;
    //
    //     isFalse = !isFalse;
    //     isFalse.SetToButton(CurrentProgress);
    //     isFalse.SetToButton(MaxProgress);
    //
    //     (isFalse ? "/" : "Max").SetToText(CenterProgress);
    //
    //     if (!isFalse)
    //     {
    //         ImgFill.fillAmount = 1;
    //         action?.Invoke();
    //         return;
    //     }
    //
    //     try
    //     {
    //         CurrentProgress.text = value.Current.ToString();
    //         MaxProgress.text = value.Max.ToString();
    //         ImgFill.fillAmount = (float)value.Current / value.Max;
    //     }
    //     catch
    //     {
    //         Debug.LogError("进肚时异常！");
    //     }
    // }
    public string IFCHANGE
    {
        get
        {
            return default;
        }
        set
        {
            Debug.LogError("大设置！");
            try
            {
                ImgFill.fillAmount = float.Parse(CurrentProgress.text) / int.Parse(MaxProgress.text);
            }
            catch
            {

            }
        }
    }

    /// <summary>
    /// 进肚条大肉棒
    /// </summary>
    public Progress(RectTransform _Slider)
    {       
            Slider =  _Slider;
    }
}
/// <summary>
/// 物体信息类容器
/// </summary>
public class ItemInfo
{
    public int ItemID;
    public int ItemSubID;
    public long ItemCount;
    public int QualityType;

    /// <summary>
    /// 构造物品
    /// </summary>
    /// <param name="item_id">物品类型ID</param>
    /// <param name="item_sub_id">物品子ID</param>
    /// <param name="item_count">物品数量</param>
    public ItemInfo(int item_id, int item_sub_id, long item_count)
    {
        ItemID = item_id;
        ItemSubID = item_sub_id;
        ItemCount = item_count;
    }
    public ItemInfo(int item_id, int item_sub_id, long item_count,int quality_type)
    {
        ItemID = item_id;
        ItemSubID = item_sub_id;
        ItemCount = item_count;
        QualityType = quality_type;
    }

    /// <summary>
    /// <空>无</空>
    /// </summary>
    public static readonly ItemInfo Zero = new ItemInfo(0, 0, 0);
    /// <summary>
    /// 金币物品
    /// </summary>
    public static readonly ItemInfo Coin = new ItemInfo(0, 2, 0);
    /// <summary>
    /// <活跃值>宝光</活跃值>
    /// </summary>
    public static readonly ItemInfo Orb = new ItemInfo(0, 12, 0);
    /// <summary>
    /// 宝珠物品
    /// </summary>
    public static readonly ItemInfo Glory = new ItemInfo(0, 14, 0);
}
public class BuffInfo
{
    public int id;//Buff ID
    public int oldValue;
    public int newValue;//加成数值
    public int buff_type;//Buff类型
    public UInt32 expire_time;//过期时间戳
    public string name;//Buff详细渠道
    public int value;//各渠道加成值

    public Dictionary<int, BuffIndex> roadBuff = new Dictionary<int, BuffIndex>
    {
        { 1,new BuffIndex(GamePlayer. FeatureNames[1],0) },
        { 2,new BuffIndex(GamePlayer. FeatureNames[2],0) },
        { 3,new BuffIndex(GamePlayer. FeatureNames[3],0) },
        { 4,new BuffIndex(GamePlayer. FeatureNames[4],0) },
        { 5,new BuffIndex(GamePlayer. FeatureNames[5],0) },
    };

    public BuffInfo()
    {

    }
    public BuffInfo(int _id, int _oldValue, int _newValue)
    {
        id = _id;
        oldValue = _oldValue;
        newValue = _newValue;
    }

    public BuffInfo(List<BuffIndex> _roadValue, int _id, int _newValue, uint _expire_time)
    {
        id = _id;
        newValue = _newValue;
        expire_time = _expire_time;

        if (_roadValue == null || _roadValue.Count < 0)
            return;
        foreach(var dic in roadBuff.Values)
        {
            dic.value = _roadValue.Find(n => n.name == dic.name).value;
        }
    }
    public BuffInfo(string _name ,int _value)
    {
        name = _name;
        value = _value;
    }
}

public class BuffIndex
{
    public string name;//Buff详细渠道
    public int value;//各渠道加成值
    public BuffIndex(string _name, int _value)
    {
        name = _name;
        value = _value;
    }
}
public class ItemUsingInfo
{
    public ItemInfo itemInfo;
    public int cooldown;
    public ulong endTime;
    public int seatId;

    public ItemUsingInfo(int item_id, int item_sub_id, long item_count, int cd, ulong end_time, int seat_id)
    {
        itemInfo = new ItemInfo(item_id, item_sub_id, item_count);
        cooldown = cd;
        endTime = end_time;
        seatId = seat_id;
    }
}

public class GamePlayer
{
    public int UserID;
    public string NickName;
    public EnumPlayerGenderType Gender;
    public int Level;
    public string Phone;
    public long integral;
    public int Head;
    public int HeadFrame;
    public int Title;
    public int Badge;
    public int Nameplate;
    public List<ItemInfo> ItemList;
    public List<BuffInfo> BuffList;
    
    public uint CreatePlayTime;//号创建时间戳
    
    public GamePlayer(CLGTLoginAck ack)
    {
        UserID = ack.user_id;
        NickName = ack.nickname;
        Gender = (EnumPlayerGenderType)ack.gender;
        Phone = ack.phone;
        Head = ack.head;
        HeadFrame = ack.head_frame;
        Title = ack.title;
        Badge = ack.badge;
        Nameplate = ack.nameplate;
        ItemList = new List<ItemInfo>();
        
        foreach (var item in ack.items)
        {
            // Debug.LogError("（获取物品数据）ack.id: " + item.item_id+" ,ack.suid: "+item.item_sub_id+" ,ack.count: "+item.item_count);
            var t = new ItemInfo(item.item_id, item.item_sub_id, item.item_count);
            //if (item.item_sub_id ==2)
            //{
            //    Debug.LogError("获取成功到了3012！");
            //}
            //Debug.LogError(">>>>ItemID" + t.ItemID+","+ t.ItemSubID);
            var p = ItemList.Find(a => a.ItemID == t.ItemID && a.ItemSubID == t.ItemSubID);
            if (!ReferenceEquals(p, null))
                p.ItemCount = t.ItemCount;
            else
                ItemList.Add(t);
        }
        TimeHelper.SetServerTimestamp((ulong)ack.server_timestamp * 1000);
        // Debug.LogError("player 参数:"+ ack.extra_params);
        if (!string.IsNullOrEmpty(ack.extra_params))
        {
            var paramsContainer = JObject.Parse(ack.extra_params);
            var max_gun_value = 0;
            _tryGetParameterFromJson(paramsContainer, "max_gun_value", out max_gun_value);

            _tryGetParameterFromJson(paramsContainer, "create_time", out CreatePlayTime);
            Debug.LogWarning("该号创建时间:(新加)"+ CreatePlayTime);
        }
        else
        {
            Debug.LogWarning("extra_params 为空,跳过解析扩展参数");
        }
        
    }

    public static Dictionary<int, string> FeatureNames = new Dictionary<int, string>
    {
        {1, "VIP"},
        {2, "月卡"},
        {3, "锻造"},
        {4, "炮台"},
        {5, "翅膀"}
    };
    public int GetHighestDigit(int number)
    {
        // 处理负数情况，先转为正数
        number = Mathf.Abs(number);

        // 处理0的情况
        if (number == 0)
            return 0;

        // 循环除以10直到得到最高位
        while (number >= 10)
        {
            number /= 10;
        }

        return number;
    }


    private bool _tryGetParameterFromJson<T>(JObject paramsContainer, string paramName, out T value)
    {
        bool success = true;
        value = default(T);
        try
        {
            JToken token = paramsContainer[paramName];
            if (token == null)
                throw new Exception("成员不存在");
            value = token.ToObject<T>();
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogWarning($"解析LoginAck数据失败，'{paramName}':{ex.Message}");
            success = false;
        }
        return success;
    }

    public long Integral
    {
        get
        {
            return integral / SysDefines.ScoreViewPara;
        }
    }
}


/// <summary>
/// 自定义包含索引和Transform组件的类
/// 用于存储Transform及其在列表中的位置信息
/// </summary>
public class IndexedTransform
{
    //
    public static int AutomaticValue=0;

    // 当前Transform在列表中的索引
    public int Index { get; }

    // 当前Transform在列表中的页签Id
    public int Label { get; }

    // 当前Transform在列表中的页签权重或数值
    public long Value { set; get; }

    //使用中的子节点
    public bool IsUsable { get; set; }

    // 对应的Transform组件
    public Transform Transform { get; }

    // 对应的GameObject组件
    public GameObject GameObject { get; }

    // 相关联的UI界面
    public Transform UIContent { get; }
    public Toggle Toggle => Transform.GetOrAddComponent<Toggle>();

    public ToggleGroup ToggleGroup => Transform.parent.GetOrAddComponent<ToggleGroup>();
    

    /// <summary>
    /// 构造函数：初始化索引和Transform
    /// </summary>
    /// <param name="index">索引值（通常是在列表中的位置）</param>
    /// <param name="transform">对应的Transform组件（不能为空）</param>
    public IndexedTransform(Transform transform, int index=-1)
    {
        // 索引合法性检查
        //if (index < 0)
        //    throw new System.ArgumentOutOfRangeException(nameof(index), "索引不能为负数");

        // Transform合法性检查
        if (transform == null)
            throw new System.ArgumentNullException(nameof(transform), "Transform组件不能为空");

        Index = index < 0 ? AutomaticValue++ : index;

        Label = Index + 1;

        Transform = transform;
        GameObject = transform.gameObject;
    }
    /// <summary>
    /// 构造函数：初始化索引和Transform
    /// </summary>
    /// <param name="index">索引值（通常是在列表中的位置）</param>
    /// <param name="transform">对应的Transform组件（不能为空）</param>
    public IndexedTransform(GameObject gameObject, int index = -1)
    {
        // 索引合法性检查
        //if (index < 0)
        //    throw new System.ArgumentOutOfRangeException(nameof(index), "索引不能为负数");

        // Transform合法性检查
        if (gameObject.transform == null)
            throw new System.ArgumentNullException(nameof(gameObject.transform), "Transform组件不能为空");

        Index = index < 0 ? AutomaticValue++ : index;

        Label = Index + 1;

        Transform = gameObject.transform;
        GameObject = gameObject;
    }

    /// <summary>
    /// 构造函数：初始化索引和Transform
    /// </summary>
    /// <param name="index">索引值（通常是在列表中的位置）</param>
    /// <param name="transform">对应的Transform组件（不能为空）</param>
    public IndexedTransform(Transform transform,Transform UI, int index = -1)
    {
        // 索引合法性检查
        //if (index < 0)
        //    throw new System.ArgumentOutOfRangeException(nameof(index), "索引不能为负数");

        // Transform合法性检查
        if (transform == null)
            throw new System.ArgumentNullException(nameof(transform), "Transform组件不能为空");

        Index = index < 0 ? AutomaticValue++ : index;

        Label = Index + 1;

        Transform = transform;
        GameObject = transform.gameObject;
        UIContent = UI;
    }

    /// <summary>
    /// 移除按钮
    /// </summary>
    public void Remove()
    {
        // false.SetToButton(Transform);
    }

    /// <summary>
    /// 构造函数：初始化索引和Transform
    /// </summary>
    /// <param name="index">索引值（通常是在列表中的位置）</param>
    /// <param name="transform">对应的Transform组件（不能为空）</param>
    public IndexedTransform(GameObject gameObject, GameObject UI, int index = -1)
    {
        // 索引合法性检查
        //if (index < 0)
        //    throw new System.ArgumentOutOfRangeException(nameof(index), "索引不能为负数");

        // Transform合法性检查
        if (gameObject.transform == null)
            throw new System.ArgumentNullException(nameof(gameObject.transform), "Transform组件不能为空");

        Index = index < 0 ? AutomaticValue++ : index;

        Label = Index + 1;

        Transform = gameObject.transform;
        GameObject = gameObject;
        UIContent = UI.transform;
    }

    // 快捷访问：通过索引器直接访问Transform的属性（简化调用）
    public Transform this[int i] => Transform.GetChild(i);

    // 快捷访问：直接获取Transform的名称（避免频繁写 .Transform.name）
    public string Name => Transform.name;

    // 显式转换为Transform（需要时可直接转为原生Transform）
    public static explicit operator Transform(IndexedTransform indexedTransform)
    {
        return indexedTransform.Transform;
    }

    // 重写ToString，方便调试时查看信息
    public override string ToString()
    {
        // return $"IndexedTransform [索引: {Index}, 名称: {Transform.name}, 路径: {Transform.GetFullPath()}]";
        return null;
    }
    
}
// 定义tag结构体，用于枚举到int的转换
public struct Tag
{
    private readonly int _value;

    private Tag(int value)
    {
        _value = value;
    }

    // 允许将tag隐式转换为int
    public static implicit operator int(Tag t)
    {
        return t._value;
    }

    // 允许将任意枚举类型显式转换为tag
    public static explicit operator Tag(Enum enumValue)
    {
        return new Tag(Convert.ToInt32(enumValue));
    }
}

/// <summary>
/// 入口条件密封数据结构（数据不可变，仅支持初始化时设置）
/// </summary>
public struct EntryCondition
{
    // 只读属性：确保数据初始化后不可修改（密封数据）
    public int SmOrder { get; }      // 排序
    public int OpenType { get; }     // 解锁类型
    public int OpenNum { get; }      // 解锁参数值
    public int ShowEnter { get; }    // 是否展示入口（0/1）
    public string Tips { get; }      // 提示文本
    public int WindowUi { get; }     // 是否弹窗（0/1）

    /// <summary>
    /// 构造函数：仅允许通过构造初始化数据（密封数据的核心，禁止后续修改）
    /// </summary>
    public EntryCondition(int smOrder, int openType, int openNum, int showEnter, string tips, int windowUi)
    {
        SmOrder = smOrder;
        OpenType = openType;
        OpenNum = openNum;
        ShowEnter = showEnter;
        Tips = tips ?? throw new ArgumentNullException(nameof(tips), "提示文本不可为null");
        WindowUi = windowUi;
    }
}
















#region 容器类大全

//炮台信息类容器
public class GunInfo
{
    public int Id;
    public int Star;
    public uint Expire_Time;

    public GunInfo(int id, int star, uint expire_time)
    {
        Id = id;
        Star = star;
        Expire_Time = expire_time;
    }
}

//翅膀信息类容器
public class WingsInfo
{
    public int Id;
    public uint Expire_Time;

    public WingsInfo(int id, uint expire_time)
    {
        Id = id;
        Expire_Time = expire_time;
    }
}

//锻造信息类容器
public class ForgeBuffInfo
{
    public int StateType;
    public string NewValue;
    public string Describe;

    public ForgeBuffInfo(int stateType, string newValue, string describe)
    {
        StateType = stateType;
        NewValue = newValue;        
        Describe = describe;
    }
}

#endregion
