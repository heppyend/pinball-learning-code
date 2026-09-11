using JBPROTO;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class ErrcodeHelper
{
    private static Dictionary<int, TLanguageErrcode> cacheDataMap_;
    private static Dictionary<string, string> cacheLanguages_;

    private static void _makesureCacheValidate()
    {
        // if (cacheDataMap_ == TLanguageErrcodeHelper.DataMap)
        //     return;

        cacheDataMap_ = TLanguageErrcodeHelper.DataMap;
        cacheLanguages_ = new Dictionary<string, string>();
        if (cacheDataMap_ != null)
        {
            foreach (TLanguageErrcode t in cacheDataMap_.Values)
                cacheLanguages_[t.key] = t.CN;
        }
    }

    public static string GetText(string protoName, int errcode)
    {
        _makesureCacheValidate();

        if (errcode == -1)
            return "请求太频繁，请稍后再试。";

        var key = $"{protoName}_{errcode}";
        string r = null;
        return cacheLanguages_.TryGetValue(key, out r) ? r : $"未知错误：{key}";
    }

    public static string GetText<T>(int errcode) where T : INetProtocol
    {
        return GetText(typeof(T).Name, errcode);
    }

    public static string GetText(INetProtocol protoObject)
    {
        var t = protoObject.GetType();
       
        var info = t.GetField("errcode");

       // Debug.LogError("GetText t:" + t.Name+","+ (sbyte)info.GetValue(protoObject));
        if (info == null)
            return $"未知错误：{t.Name}";

        var code = (sbyte)info.GetValue(protoObject);
        return GetText(t.Name, code);
    }
}
