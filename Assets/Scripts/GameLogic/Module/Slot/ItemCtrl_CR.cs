using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace QT.Module.Slot
{
    /*由上往下滚动
     * image
     * 1、每列image总数比每列可见image数多2个
     * 2、用到锚点坐标
     */
    //如5*3滚动
    //0  3  6  9   12
    //1  4  7  10  13
    //2  5  8  11  14
    public class ItemCtrl_CR
    {
        private enum TransCtrl
        {
            TC_NONR = 0,              //待机
            TC_Trans,                 //滚动阶段
            TC_End,                   //结束阶段
        }
        private TransCtrl m_iTransCtrl = TransCtrl.TC_NONR;   //滚动流程控制变量

        public delegate void CallBackEvn();
        protected CallBackEvn AllStopBackEvn;         //全部滚停回调
        public delegate void CallBackEvn_1(int iCol);
        protected CallBackEvn_1 AColStopBackEvn;      //单列滚停回调

        private float m_fImageHeight = 0;                   //相邻的两image中心点距离

        //he_add_20180908
        private float[] m_ary_fTransSpeed;                  //默认滚动速度
        private float[] m_ary_fTransSpeedNow;               //实时速度

        private int[,] m_arys_iItemResult;                  //Item结果
        private int[] m_ary_iShowResultIdTemp;              //每列有几个item已经设置了正确结果
        private bool[] m_ary_bIsCanStop;                    //对应的列是否可以停止
        private bool[] m_ary_bIsStoped;                     //对应的列是否已经停止

        //=======抛物线=======
        private float m_fX1 = 0;
        private float m_fX2 = 0;
        private int m_fParabolaHeight = 0;                  //抛物线高度
        private int m_iParabolaReboundSpeed = 0;            //抛物线回弹速度
        private float[] m_ary_fParabolaXTemp;               //抛物线x(y=x*x-h)
        private bool m_bIsShowParabola;                     //是否显示抛物线效果
        private bool[] m_ary_bIsCanShowParabola;            //是否可以显示抛物线效果
        //====================

        private int m_iStopedCount = 0;                     //已经停止的总列数

        private Sprite[] m_ary_SpItemRes;                           //需要被加载的Item图片
        private Dictionary<int, List<Image>> m_dic_ListImage;       //image
        private Vector2[] m_ary_VecStartAncPos;                     //image的原始锚点坐标
        
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="ary_SpItemRes">需要被加载的Item图片资源</param>
        /// <param name="arys_ImageItem">item的image组件（image_Item.GetLength(1)个数应该为：row+2）</param>
        /// <param name="allStopBackEvn">全部滚停回调函数</param>
        /// <param name="aColStopBackEvn">单列滚动回调函数</param>
        /// <param name="fTransSpeed">默认滚动速度</param>
        /// <param name="bIsShowParabola">是否显示弹簧效果</param>
        /// <param name="iParabolaReboundSpeed">弹簧回弹速度</param>
        /// <param name="fParabolaHeightPerc">弹簧高度百分比（两张相邻image距离的百分比）0-1</param>
        /// <returns></returns>
        public ItemCtrl_CR(Sprite[] ary_SpItemRes, Image[,] arys_ImageItem, CallBackEvn allStopBackEvn = null, CallBackEvn_1 aColStopBackEvn = null,
                           float fTransSpeed = 100, bool bIsShowParabola = true, int iParabolaReboundSpeed = 40, float fParabolaHeightPerc = 0.3f)
        {
            if (ary_SpItemRes == null || arys_ImageItem == null || fTransSpeed < 0)
            {
               // Debuger.LogWarning("[ItemCtrl]-> 初始化失败！");
                return;
            }
            if (iParabolaReboundSpeed <= 0) iParabolaReboundSpeed = 40;
            fParabolaHeightPerc = Mathf.Clamp(fParabolaHeightPerc, 0, 1);

            m_ary_SpItemRes = ary_SpItemRes;
            m_dic_ListImage = new Dictionary<int, List<Image>>();
            for (int i = 0; i < arys_ImageItem.GetLength(0); i++)
            {
                m_dic_ListImage.Add(i, new List<Image>());
                for (int j = 0; j < arys_ImageItem.GetLength(1); j++)
                //    m_dic_ListImage[i].Add(arys_ImageItem[i,  j]);
                 m_dic_ListImage[i].Add(arys_ImageItem[i, arys_ImageItem.GetLength(1) - j-1]);
            }
            m_ary_VecStartAncPos = new Vector2[arys_ImageItem.GetLength(1)];
            for (int i = 0; i < m_ary_VecStartAncPos.Length; i++)
           //     m_ary_VecStartAncPos[i] = arys_ImageItem[0, i].rectTransform.anchoredPosition;
            m_ary_VecStartAncPos[i] = arys_ImageItem[0, m_ary_VecStartAncPos.Length - i -1].rectTransform.anchoredPosition;


            m_fImageHeight = Mathf.Abs(m_ary_VecStartAncPos[1].y - m_ary_VecStartAncPos[0].y);

         //   Debug.LogError(" m_ary_VecStartAncPos[0].y :" + m_ary_VecStartAncPos[0].y + ",m_fImageHeight:" + m_fImageHeight+","+ m_dic_ListImage[0][0].name);

         //   Debug.LogError(" 第一个的位置 :"+ m_ary_VecStartAncPos[0].y + ",最后一个位置," + m_ary_VecStartAncPos[m_ary_VecStartAncPos.Length - 1].y);
            AllStopBackEvn = allStopBackEvn;
            AColStopBackEvn = aColStopBackEvn;
            m_bIsShowParabola = bIsShowParabola;
            m_iParabolaReboundSpeed = iParabolaReboundSpeed;
            m_fParabolaHeight = (int)(m_fImageHeight * fParabolaHeightPerc);
            int col = arys_ImageItem.GetLength(0);
            int row = arys_ImageItem.GetLength(1) - 2;
            m_ary_fTransSpeed = new float[col];
            m_ary_fTransSpeedNow = new float[col];
            for (int i = 0; i < m_ary_fTransSpeed.Length; i++)
            {
                m_ary_fTransSpeed[i] = fTransSpeed;
                m_ary_fTransSpeedNow[i] = fTransSpeed;
            }
            m_ary_iShowResultIdTemp = new int[col];
            m_arys_iItemResult = new int[col, row];
            m_ary_bIsCanStop = new bool[col];
            m_ary_bIsStoped = new bool[col];
            m_ary_fParabolaXTemp = new float[col];
            m_ary_bIsCanShowParabola = new bool[col];
            m_fX1 = -Mathf.Sqrt(m_fParabolaHeight);
            m_fX2 = -m_fX1;
        }

        /// <summary>
        /// 状态机，在update()调用
        /// </summary>
        public void StateItemCtrl()
        {
            switch (m_iTransCtrl)
            {
                case TransCtrl.TC_NONR:
                    break;
                case TransCtrl.TC_Trans:
                    m_iStopedCount = 0;
                    for (int i = 0; i < m_ary_bIsStoped.Length; i++)
                    {
                        if (m_ary_bIsStoped[i])
                            m_iStopedCount++;
                    }
                    if (m_iStopedCount == m_ary_bIsStoped.Length)
                    {
                        m_iTransCtrl = TransCtrl.TC_End;
                        return;
                    }
                    for (int i = 0; i < m_ary_bIsCanStop.Length; i++)
                    {
                        if (!m_ary_bIsStoped[i])
                            OnTransing(i, m_ary_bIsCanStop[i], m_bIsShowParabola);
                    }
                    break;
                case TransCtrl.TC_End:
                    m_iTransCtrl = TransCtrl.TC_NONR;
                    if (AllStopBackEvn != null)
                        AllStopBackEvn();
                    break;
            }
        }

        /// <summary>
        /// 初始化显示全部结果sprite
        /// </summary>
        /// <returns></returns>
        public void ShowInitResultSpr()
        {
            for (int i = 0; i < m_dic_ListImage.Count; i++)
            {
                for (int j = 0; j < m_dic_ListImage[i].Count; j++)
                {
                    if (j > 0 && j < m_dic_ListImage[i].Count - 1)
                        m_dic_ListImage[i][j].sprite = m_ary_SpItemRes[m_arys_iItemResult[i, m_arys_iItemResult.GetLength(1) - j]];
                    else
                        m_dic_ListImage[i][j].sprite = m_ary_SpItemRes[UnityEngine.Random.Range(0, m_ary_SpItemRes.Length - 1)];
                }
            }
        }

        /// <summary>
        /// 启动滚动
        /// </summary>
        public void OnTrans()
        {
            if (m_iTransCtrl != TransCtrl.TC_NONR) return;
            for (int i = 0; i < m_arys_iItemResult.GetLength(0); i++)
            {
                for (int j = 0; j < m_arys_iItemResult.GetLength(1); j++)
                {
                    m_arys_iItemResult[i, j] = -1;
                }
                m_ary_iShowResultIdTemp[i] = 0;
                m_ary_fParabolaXTemp[i] = 0;
                m_ary_bIsCanStop[i] = false;
                m_ary_bIsStoped[i] = false;
                m_ary_bIsCanShowParabola[i] = false;
                m_ary_fTransSpeedNow[i] = m_ary_fTransSpeed[i];
            }
            m_iTransCtrl = TransCtrl.TC_Trans;
        }

        /// <summary>
        /// 设置全部Item结果
        /// </summary>
        /// <param name="arys_iResult">item结果数据</param>
        /// <returns></returns>
        public void SetAllItemResult(int[,] arys_iResult)
        {
            

            if (arys_iResult == null || arys_iResult.Length != m_arys_iItemResult.Length)
            {
               // Debuger.LogWarning("[ItemCtrl_CR]-> SetAllItemResult() 设置结果失败");
                return;
            }
            for (int i = 0; i < arys_iResult.GetLength(0); i++)
            {
                for (int j = 0; j < arys_iResult.GetLength(1); j++)
                {
                   // Debug.LogError("==SetAllItemResult==="+",i="+i+",j="+j +",结果:"+ arys_iResult[i, j]);
                    m_arys_iItemResult[i, j] = arys_iResult[i, j];
                }
            }
        }

        /// <summary>
        /// 设置一个Item结果
        /// </summary>
        /// <param name="col">列</param>
        /// <param name="row">行</param>
        /// <param name="iResult">Item的结果</param>
        /// <returns></returns>
        public void SetOneItemResult(int col, int row, int iResult)
        {
            if (col < 0 || col >= m_arys_iItemResult.GetLength(0) || row < 0 || row >= m_arys_iItemResult.GetLength(1) || iResult < 0 || iResult >= m_ary_SpItemRes.Length)
            {
              //  Debuger.LogWarning("[ItemCtrl_CR]-> SetOneResultImage() 传入参数有误！col：" + col + "    row" + row + "  iResult：" + iResult);
                return;
            }
            else
            {
                m_arys_iItemResult[col, row] = iResult;
                Debug.LogError("===SetOneItemResult:" + iResult);
                m_dic_ListImage[col][m_dic_ListImage[col].Count - 2 - row].sprite = m_ary_SpItemRes[m_arys_iItemResult[col, row]];
            }
        }

        /// <summary>
        /// 全部停止
        /// </summary>
        /// <returns></returns>
        public void SetAllItemStop()
        {
            if (m_iTransCtrl != TransCtrl.TC_Trans) return;
            else
            {
                for (int i = 0; i < m_ary_bIsCanStop.Length; i++)
                    m_ary_bIsCanStop[i] = true;
            }
        }

        /// <summary>
        /// 单列停止
        /// </summary>
        /// <param name="col">item的列数，0 - col-1</param>
        /// <returns></returns>
        public void SetAColItemStop(int col)
        {
            if (m_iTransCtrl != TransCtrl.TC_Trans || col < 0 || col >= m_ary_bIsCanStop.Length)
            {
              //  Debuger.LogWarning("[ItemCtrl_CR]-> SetAColItemStop() 失败！m_iTransCtrl= " + m_iTransCtrl + "   col：" + col);
                return;
            }
            else
                m_ary_bIsCanStop[col] = true;
        }

        /// <summary>
        /// 返回对应列是否可以停止
        /// </summary>
        /// <param name="col">item的列数，0 - col-1</param>
        /// <returns></returns>
        public bool Get_m_ary_bIsCanStop(int col)
        {
            if (col < 0 || col >= m_ary_bIsCanStop.Length)
            {
              //  Debuger.LogWarning("[ItemCtrl_CR]-> Get_m_ary_bIsCanStop() 传入参数有误：col：" + col);
                return false;
            }
            return m_ary_bIsCanStop[col];
        }

        /// <summary>
        /// 返回对应列是否已经停止(显示结果)
        /// </summary>
        /// <param name="col">列</param>
        /// <returns></returns>
        public bool Get_m_ary_bIsStoped(int col)
        {
            if (col < 0 || col >= m_ary_bIsStoped.Length)
            {
               // Debuger.LogWarning("[ItemCtrl_CR]-> Get_m_ary_bIsStoped() 传入参数有误：col：" + col);
                return false;
            }
            else
                return m_ary_bIsStoped[col];
        }

        /// <summary>
        /// 获取image
        /// </summary>
        /// <param name="col">列</param>
        /// <param name="row">行</param>
        /// <returns></returns>
        public Image GetImage(int col, int row)
        {
            if (col < 0 || col >= m_arys_iItemResult.GetLength(0) || row < 0 || row >= m_arys_iItemResult.GetLength(1))
            {
               // Debuger.LogWarning("[ItemCtrl_CR_Double]-> GetImage() 传入参数有误！col：" + col + "    row" + row);
                return null;
            }
            return m_dic_ListImage[col][m_dic_ListImage[col].Count - 2 - row];
        }

        /// <summary>
        /// 本次滚动是否已经结束,(如果设置了回调函数，可以忽略)
        /// </summary>
        public bool Get_IsAllItemTransEnd
        {
            get { return m_iTransCtrl == TransCtrl.TC_NONR; }
        }

        /// <summary>
        /// 修改实时滚动速度
        /// </summary>
        /// <param name="iCol">列</param>
        /// <param name="fPre">新设置速度与默认速度的百分比</param>
        public void SetModifySpeed(int iCol, float fPre = 0.5f)
        {
            if (iCol < 0 || iCol > m_ary_fTransSpeedNow.Length || fPre <= 0)
                return;
            m_ary_fTransSpeedNow[iCol] = m_ary_fTransSpeed[iCol] * fPre;
        }


        /// <summary>
        /// 滚动
        /// </summary>
        /// <param name="col">item的列</param>
        /// <param name="bIsCanStop">是否可以停止</param>
        /// <param name="bIsShowParabola">是否需要显示抛物线效果</param>
        /// <returns></returns>
        private void OnTransing(int col, bool bIsCanStop, bool bIsShowParabola)
        {
            if (m_ary_iShowResultIdTemp[col] < m_arys_iItemResult.GetLength(1))
            {
                OnTransLogic(col, bIsCanStop);
            }
            else
            {
                if (!m_ary_bIsCanShowParabola[col])
                {
                    OnTransLogic(col, false);
                }
                else       //显示抛物线效果
                {
                    if (bIsShowParabola)
                    {
                        ShowParabolaEff(col, ParabolaLogic(col, m_fParabolaHeight));
                    }
                    else
                    {
                        m_ary_bIsStoped[col] = true;
                        ShowParabolaEff(col, 0);
                        if (AColStopBackEvn != null)
                            AColStopBackEvn(col);
                    }
                }

            }
        }

        /// <summary>
        /// 滚动实现逻辑
        /// </summary>
        /// <param name="col">item的列</param>
        /// <param name="bIsCanStop">是否可以停止</param>
        /// <returns></returns>
        private void OnTransLogic(int col, bool bIsCanStop)
        {
            float fSpeed = m_ary_fTransSpeedNow[col] * Time.deltaTime;
            for (int i = 0; i < m_dic_ListImage[col].Count; i++)
            {
                m_dic_ListImage[col][i].rectTransform.anchoredPosition += new Vector2(0, fSpeed);
            }
            Image imageTemp = m_dic_ListImage[col][0];
            if (imageTemp.rectTransform.anchoredPosition.y >= m_ary_VecStartAncPos[0].y + m_fImageHeight)
            {
               
                if (m_ary_iShowResultIdTemp[col] >= m_arys_iItemResult.GetLength(1))
                {
                    m_ary_bIsCanShowParabola[col] = true;
                    bIsCanStop = false;                          //加这句是防止外面设置错误
                }

                if (bIsCanStop)
                {
                    if (m_arys_iItemResult[col, m_arys_iItemResult.GetLength(1) - 1] != -1)         //这样写是为了确保整列结果都不为-1
                    {
                        //Debug.LogError("col:"+ col+ ",中间结果:"+(2- m_ary_iShowResultIdTemp[col])+ ",结果:" + m_arys_iItemResult[col, m_arys_iItemResult.GetLength(1) - 1 - m_ary_iShowResultIdTemp[col]]);

                       // imageTemp.sprite = m_ary_SpItemRes[m_arys_iItemResult[col, m_arys_iItemResult.GetLength(1) - 1 - m_ary_iShowResultIdTemp[col]]];
                        m_ary_iShowResultIdTemp[col]++;
                    }
                }
                else
                {
                  //  imageTemp.sprite = m_ary_SpItemRes[UnityEngine.Random.Range(0, m_ary_SpItemRes.Length - 1)];
                }
                //float fAdjustment = imageTemp.rectTransform.anchoredPosition.y - m_ary_VecStartAncPos[m_ary_VecStartAncPos.Length - 1].y - m_fImageHeight;
                //imageTemp.rectTransform.anchoredPosition = new Vector2(m_ary_VecStartAncPos[0].x, m_ary_VecStartAncPos[0].y - fAdjustment);


                float fAdjustment = imageTemp.rectTransform.anchoredPosition.y + m_ary_VecStartAncPos[0].y - m_fImageHeight;

                //Debug.LogError("=====name:" + imageTemp.name + "," + imageTemp.rectTransform.anchoredPosition.y+ ",fAdjustment:" + fAdjustment);

                imageTemp.rectTransform.anchoredPosition = new Vector2(m_ary_VecStartAncPos[m_ary_VecStartAncPos.Length - 1].x, m_ary_VecStartAncPos[m_ary_VecStartAncPos.Length - 1].y + 0);

                m_dic_ListImage[col].RemoveAt(0);
                m_dic_ListImage[col].Add(imageTemp);
            }
        }

        /// <summary>
        /// 抛物线实现逻辑 y=x*x-h
        /// </summary>
        /// <param name="col">item的列</param>
        /// <param name="fParabolaHeight">缓冲高度</param>
        /// <returns></returns>
        private float ParabolaLogic(int col, float fParabolaHeight)
        {
            m_ary_fParabolaXTemp[col] += Time.deltaTime * m_iParabolaReboundSpeed;
            float x = m_fX1 + m_ary_fParabolaXTemp[col];
            x = x < m_fX1 ? m_fX1 : x;
            x = x > m_fX2 ? m_fX2 : x;
            if (x >= m_fX2)
            {
                m_ary_bIsStoped[col] = true;
                if (AColStopBackEvn != null)
                    AColStopBackEvn(col);
            }
            return x * x - fParabolaHeight;
        }

        /// <summary>
        /// 显示抛物线效果
        /// </summary>
        /// <param name="col">item的列</param>
        /// <param name="transY"></param>
        /// <returns></returns>
        private void ShowParabolaEff(int col, float transY)
        {
            for (int i = 0; i < m_dic_ListImage[col].Count; i++)
            {
                if (m_ary_bIsStoped[col])
                {
                    m_dic_ListImage[col][i].rectTransform.anchoredPosition = new Vector2(m_ary_VecStartAncPos[i].x, m_ary_VecStartAncPos[i].y);
                }
                else
                {
                    m_dic_ListImage[col][i].rectTransform.anchoredPosition = new Vector2(m_ary_VecStartAncPos[i].x, m_ary_VecStartAncPos[i].y + transY);
                }
            }
        }

    }

}
