using JBPROTO;
using LC.Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CommonModule : BaseModule
{
    public CommonModule()
    {
        AutoRegister = true;
    }

    private int waitLockCount = 0;
    public int WaitLockCount
    {
        get { return waitLockCount; }
        set
        {
            //if (value > 0)
            //    UIManager.Instance.OpenUI(EnumUIType.WaitResponseUI);
            //else
            //    UIManager.Instance.CloseUI(EnumUIType.WaitResponseUI);
            waitLockCount = value;
        }
    }

    protected override void OnLoad()
    {
    }

    protected override void OnRelease()
    {
    }

    public void Update()
    {
        //临时屏蔽
        //if (!openUI)
        //{
        //    if (!ReferenceEquals(circleBroadcastList, null) && circleBroadcastList.Count > 0)
        //    {
        //        var serverTime = TimeHelper.GetServerTimestamp();
        //        if (circleBroadcastList.First().Timestamp <= serverTime)
        //        {
        //            UIManager.Instance.OpenUI(EnumUIType.SystemNoticeUI);
        //            openUI = true;
        //        }
        //    }
        //}
    }
}

public class CircleBroadcastMessage
{
    public int Id;
    public ulong Timestamp;
    public string Message;
}