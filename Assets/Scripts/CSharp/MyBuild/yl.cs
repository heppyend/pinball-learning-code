using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eyl
{
    land = 0,
    zjh,
}
public class yl : MonoBehaviour {

    public static Dictionary<int, AssetUpdater> updateList;   //下载中的小游戏列表

    public static List<GameInfo> _gameList;

    public static bool APPSTORE_VERSION = false;
    public static Dictionary<int, string> GAME_NOTICE_TYPE = new Dictionary<int, string>()
    {
        { (int)(eyl.land), "斗地主" },
        {(int)(eyl.zjh),"炸金花"},
        //{(int)(eyl.oxbattle), "百人牛牛"},
        //{(int)(eyl.oxnew), "抢庄牛牛"},
        //{(int)(eyl.balckred), "红黑大战"},
        //{(int)(eyl.longhudou), "龙虎斗" },
        //{(int)(eyl.baccaratnew), "百家乐"},
        //{(int)(eyl.buyu), "捕鱼"},
        //{(int)(eyl.sparrowtwo), "二人麻将"},
        //{(int)(eyl.sharkbattle), "狮子王国"},
        //{(int)(eyl.sparrowxl), "血流麻将"},
        //{(int)(eyl.bcbm), "奔驰宝马"},
        //{(int)(eyl.runfasthn), "跑得快"},
        //{(int)(eyl.dezhoupuke), "德州扑克"},
        //{(int)(eyl.fruitslot), "水果机"},
        //{(int)(eyl.winplay), "连环夺宝"},
        //{(int)(eyl.baccaratnew_video), "视讯百家乐"},
        //{(int)(eyl.blackjack), "21点"},
        //{(int)(eyl.eluosizp), "俄罗斯转盘"},
        //{(int)(eyl.cqssc), "彩票"},
        //{(int)(eyl.suoha), "梭哈"},
        //{(int)(eyl.saibao), "百人骰宝"},
        //{(int)(eyl.saibao_video), "视讯骰宝"},
        //{(int)(eyl.sangong), "三公"},
        //{(int)(eyl.tuitongzi), "推筒子"},
        //{(int)(eyl.redminec), "红包"},
        //{(int)(eyl.fenfencai), "分分彩"},
        //{(int)(eyl.caidaxiao), "彩票猜大小"},
        };

}

