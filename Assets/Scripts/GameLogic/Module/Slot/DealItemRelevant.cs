using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

using QT.Module.Slot;

namespace QT.Stage.GameSlotSparrow25
{
    //滚动状态
    public enum eRollState
    {
        eRS_None = 0,   //没滚动
        eRS_Rolling,    //滚动中
    }

    //处理与item相关
    public class DealItemRelevant : MonoBehaviour
    {
        private GameSlotSparrowLogic m_Logic;
        public ItemCtrl_CR_MulRes_Spe m_ItemCtrl;                //item滚动脚本

        private eRollState m_eRollState = eRollState.eRS_None;

        //=====计时器=====
        public float m_fItemTransInter = 0.4f;             //item滚动的时间间隔
        private float m_fItemTransTimer = 0;               //item滚动计时器
        private float m_fItemStopTotalTime = 1f;           //手动玩模式中，如果玩家转动后不手动停，等待该时间后将启动自动停流程
        private float m_fItemStopTotalTimer = 0;           //计时器
        //================

        public int m_iColStop = 0;           //指定哪列停

        private bool m_bIsTransing = false;                //是否在滚动中
        public bool m_bIsGetResult = false;                //是否获取到结果,idle状态时设为false，得到中奖结果时为true
        private bool m_bIsAutoStopOpened = false;          //自动停流程是否已经开启

        // public Sound m_SoundSpin;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <returns></returns>
        public DealItemRelevant(GameSlotSparrowLogic logic)
        {
            m_Logic = logic;
        }

        //public override void Final()
        //{
        //    m_ItemCtrl = null;
        //}

        /// <summary>
        /// 状态机Updat()调用
        /// </summary>
        /// <returns></returns>
        public void UpdateState()
        {
            //Debug.LogError("UpdateState");
            m_ItemCtrl.StateItemCtrl();
        }

        /// <summary>
        /// 重置参数
        /// </summary>
        /// <returns></returns>
        public void ResetParam()
        {
            // if (m_Logic.m_DealWinPro.m_AnimMultiple.gameObject.activeInHierarchy)
            //     m_Logic.m_DealWinPro.m_AnimMultiple.SetBool("m_bPlay", false);
            m_fItemTransTimer = 0;
            m_fItemStopTotalTimer = 0;
            m_bIsTransing = false;
            m_bIsGetResult = false;
            m_eRollState = eRollState.eRS_None;
            m_bIsAutoStopOpened = false;
        }

        /// <summary>
        /// 初始化显示全部结果
        /// </summary>
        /// <returns></returns>
        public void ShowInitResultSpr()
        {
            m_ItemCtrl.ShowInitResultSpr();
        }

        /// <summary>
        /// 启动滚动
        /// </summary>
        /// <returns></returns>
        public void OnTrans()
        {
            m_ItemCtrl.OnTrans();
            //m_SoundSpin = m_Logic.m_DealSound.SEPlay((int)eAudioName.Sound_Spin, m_Logic.m_iSeatID, true, false);   //222待修改
        }

        /// <summary>
        /// 设置全部item结果
        /// </summary>
        /// <param name="arys_iResult">item结果数据</param>
        /// <returns></returns>
        public void SetAllItemResult(int[,] arys_iResult)
        {
            m_ItemCtrl.SetAllItemResult(arys_iResult);
        }

        /// <summary>
        /// 设置一个Item结果
        /// </summary>
        /// <param name="col">列</param>
        /// <param name="row">行</param>
        /// <param name="iResult">Item的结果</param>
        /// <returns></returns>
        public void SetAItemResult(int col, int row, int iResult)
        {
            m_ItemCtrl.SetOneItemResult(col, row, iResult);
        }

        /// <summary>
        /// 全部停止
        /// </summary>
        /// <returns></returns>
        public void SetAllItemStop()
        {
            m_ItemCtrl.SetAllItemStop();
            // m_Logic.m_DealSound.SEPlay((int)eAudioName.Sound_SpinStop, m_Logic.m_iSeatID, false, true);
        }

        /// <summary>
        /// 单列停止
        /// </summary>
        /// <param name="col">item的列数，0 - col-1</param>
        /// <returns></returns>
        public void SetAColItemStop(int iCol)
        {
            m_ItemCtrl.SetAColItemStop(iCol);
            // m_Logic.m_DealSound.SEPlay((int)eAudioName.Sound_SpinStop, m_Logic.m_iSeatID, false, true);
        }

        /// <summary>
        /// 获取image
        /// </summary>
        /// <param name="col">列</param>
        /// <param name="row">行</param>
        /// <returns></returns>
        public Image GetImage(int col, int row)
        {
            return m_ItemCtrl.GetImage(col, row);
        }

        /// <summary>
        /// 本次滚动是否已经结束
        /// </summary>
        /// <returns></returns>
        public bool Get_IsAllItemTransEnd()
        {
            return m_ItemCtrl.Get_IsAllItemTransEnd;
        }

        /// <summary>
        /// 返回对应列是否可以停止
        /// </summary>
        /// <param name="col">item的列数，0 - col-1</param>
        /// <returns></returns>
        public bool Get_m_ary_bIsCanStop(int col)
        {
            return m_ItemCtrl.Get_m_ary_bIsCanStop(col);
        }

        /// <summary>
        /// 处理滚动后续事件
        /// </summary>
        /// <returns></returns>
        public void DealTransStop()
        {
            switch (m_eRollState)
            {
                case eRollState.eRS_None:
                    if (m_bIsGetResult)
                    {
                        OnTrans();
                        SetAllItemResult(m_Logic.m_arys_ItemResult);
                        m_fWaitTime = 2.5f; //0.5f;
                        m_ActionWait += () => { SetTransing(); };
                        m_Logic.StartCoroutine(ActionWait());
                        m_eRollState = eRollState.eRS_Rolling;
                    }
                    break;
                case eRollState.eRS_Rolling:
                    if (m_bIsTransing)
                        ItemTransLogic();
                    break;
            }
        }

        private void SetTransing()
        {
            m_bIsTransing = true;
        }

        private Action m_ActionWait;
        private float m_fWaitTime = 0;
        private IEnumerator ActionWait()
        {
            yield return new WaitForSeconds(m_fWaitTime);
            if (m_ActionWait != null)
            {
                m_ActionWait();
                m_ActionWait = null;
            }
        }

        /// <summary>
        /// item停止滚动逻辑
        /// </summary>
        /// <returns></returns>
        private void ItemTransLogic()
        {
            if (IsAutoStopOpened())
            {
                m_fItemTransTimer += Time.deltaTime;
                if (m_fItemTransTimer >= m_fItemTransInter)
                {
                    m_fItemTransTimer = 0;
                    for (int i = 0; i < DataManager.m_iItemCol; i++)
                    {
                        if (!Get_m_ary_bIsCanStop(i))
                        {
                            SetAColItemStop(i);
                            break;
                        }
                    }
                }

            }
        }

        /// <summary>
        /// Item全停
        /// </summary>
        /// <returns></returns>
        public void AllStopItem()
        {
            if (!m_bIsTransing || !m_bIsGetResult)
                return;
            SetAllItemStop();
            m_Logic.m_eStartKey = eStartKey.eSK_None;
        }

        /// <summary>
        /// 指定某列item停
        /// </summary>
        /// <returns></returns>
        public void DesignateStopItem()
        {
            if (!m_bIsTransing || !m_bIsGetResult)
                return;
            int iCol = m_iColStop;
            if (iCol < 0 || iCol >= m_Logic.m_arys_ItemResult.GetLength(0))
                return;
            if (!Get_m_ary_bIsCanStop(iCol))
            {
                SetAColItemStop(iCol);
                m_fItemStopTotalTimer = 0;
            }

        }

        /// <summary>
        /// 自动停流程是否已经开启
        /// </summary>
        /// <returns></returns>
        private bool IsAutoStopOpened()
        {
            if (m_bIsAutoStopOpened)
                return true;
            else if (m_Logic.m_ePlayMode == ePlayMode.ePM_Auto)
            {
                m_bIsAutoStopOpened = true;
                return true;
            }
            else
            {
                m_fItemStopTotalTimer += Time.deltaTime;
                if (m_fItemStopTotalTimer >= m_fItemStopTotalTime)
                {
                    m_bIsAutoStopOpened = true;
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 一列停止后处理事件
        /// </summary>
        /// <param name="iCol"></param>
        public void AColStopEvn(int iCol)
        {
            int[,] arys_Result = m_Logic.m_arys_ItemResult;
            for (int i = 0; i < arys_Result.GetLength(1); i++)
            {
                if (arys_Result[iCol, i] == DataManager.m_iItem_11)
                {
                    // m_Logic.m_DealSound.SEPlay((int)eAudioName.Sound_Icon_9, m_Logic.m_iSeatID, false, true);
                    break;
                }
            }
        }


    }
}
