using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppConfig : MonoBehaviour
{
    //程序版本
    public static float BASE_C_VERSION = 1.13f; //@app_version

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    
// 获取域名对应的ip
public static string ReplaceHostByIP(string url_)
{
        //	if ( device.platform != "ios") {
        //		// return
        //	}
        //    string host = "";
        //    string[] strlist = url_.Split('/');
        //	if ( string.find(string.lower(strlist[1]),"http") )
        //    {
        //		if (strlist[2] == "") 
        //            host = strlist[3]
        //		else
        //            host = strlist[2]
        //}
        //	else

        //        host = strlist[1]
        //}
        //	if string.find(host,":")  then
        //       local hostlist = string.split(host,":")

        //        host = hostlist[1]
        //}

        //    local socket = appdf.req("socket.core")

        //    local addrinfo, err = socket.dns.getaddrinfo(host)
        //	if addrinfo and addrinfo[1] and addrinfo[1].addr then

        //        local addr = addrinfo[1].addr
        //		if addr == "127.0.0.1" or addr == "0.0.0.0" then
        //			return 
        //		}
        //		return string.gsub(url_, host, addr)

        //    }
        return "abc";
    }
}
