using System.Collections.Generic;
using XLua;


public class Context
{
    ////集合的运行环境
    //public static Context Hall = new Context();

    ////小游戏的运行环境
    //public static Context[] Game;

    //所有小游戏的AssetConfig
    public static Dictionary<string, AssetConfig> GameAssetConfigs = new Dictionary<string, AssetConfig>();

    public static int selectIndex = -1;

    //资源加载器
    public AssetLoader Loader { get; set; }

    //AssetBundle管理器
    public BundleManager BundleMgr { get; set; }

    //小游戏配置
    public AssetConfig Config { get; set; }
    
    //socket
    //public Client NetClient { get; set; }
}
