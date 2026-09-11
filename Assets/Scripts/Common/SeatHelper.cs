/******************************************************************************
 * 
 *  Title:  捕鱼项目
 *
 *  Version:  1.0版
 *
 *  Description:
 *
 *  Author:  monkey256
 *       
 *  Date:  2019
 * 
 ******************************************************************************/

using JBPROTO;
using System;

/// <summary>
/// 座位相关的辅助类
/// </summary>
public static class SeatHelper
{
    /// <summary>
    /// 我的服务端位置
    /// </summary>
    private static int myServerSeatPosition;

    /// <summary>
    /// 设置我的服务端位置
    /// </summary>
    /// <param name="serverPos">服务端位置</param>
    public static void SetMyServerSeatPosition(int serverPos)
    {
        if (serverPos >= 0 && serverPos <= 3)
            myServerSeatPosition = serverPos;
    }

    public static bool IsServerSeatNegative()
    {
        return myServerSeatPosition == 2 || myServerSeatPosition == 3;
    }

    /// <summary>
    /// 获取我的服务端位置
    /// </summary>
    /// <returns>我的服务端位置</returns>
    public static int GetMyServerSeatPosition()
    {
        return myServerSeatPosition;
    }

    /// <summary>
    /// 获取我的客户端位置
    /// </summary>
    /// <returns>我的客户端位置</returns>
    public static int GetMyClientSeatPosition()
    {
        return ConvertServerSeatPositionToClient(myServerSeatPosition);
    }
    
    /// <summary>
    /// 将服务端位置转换为客户端位置
    /// </summary>
    /// <param name="serverPos">服务端位置</param>
    /// <returns>客户端位置</returns>
    public static int ConvertServerSeatPositionToClient(int serverPos)
    {
        int r = serverPos;
        if (myServerSeatPosition == 2 || myServerSeatPosition == 3)
        {
            switch (serverPos)
            {
                case 0:
                    r = 2;
                    break;
                case 1:
                    r = 3;
                    break;
                case 2:
                    r = 0;
                    break;
                case 3:
                    r = 1;
                    break;

            }
        }
        return r;
    }

    /// <summary>
    /// 将客户端位置转换为服务端位置
    /// </summary>
    /// <param name="clientPos">客户端位置</param>
    /// <returns>服务端位置</returns>
    public static int ConvertClientSeatPositionToServer(int clientPos)
    {
        int r = clientPos;
        if (myServerSeatPosition == 2 || myServerSeatPosition == 3)
        {
            switch (clientPos)
            {
                case 0:
                    r = 2;
                    break;
                case 1:
                    r = 3;
                    break;
                case 2:
                    r = 0;
                    break;
                case 3:
                    r = 1;
                    break;
            }
        }
        return r;
    }

    /// <summary>
    /// 根据当前视角，将服务器角度转换为显示角度
    /// </summary>
    /// <param name="angle">服务器角度</param>
    /// <returns>显示角度</returns>
    public static float ConvertServerAngleToDisplay(int seat, int serverAngle)
    {
        float displayAngle = serverAngle;
        if (seat == 2 || seat == 3)
        {
            displayAngle = 180 + serverAngle;
        }
        return displayAngle;
    }

    /// <summary>
    /// 根据当前视角，将显示角度转换为服务器角度
    /// </summary>
    /// <param name="displayAngle">显示角度</param>
    /// <returns>服务器角度</returns>
    public static int ConvertDisplayAngleToServer(float displayAngle)
    {
        int serverAngle = (int)displayAngle;
        if (myServerSeatPosition == 2 || myServerSeatPosition == 3)
        {
            serverAngle = (int)displayAngle - 180;
        }
        return serverAngle;
    }
}
