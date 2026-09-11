//using QT.Stage.GameSlotChinaStreet;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace QT.Module.Slot
{
    //显示透明图模式
    public enum eShowAlphaMode
    {
        eSAM_None = 0,       //原始(如果在滚动中，会逐渐回到正常状态)
        eSAM_NowReResult,    //立刻恢复结果
        eSAM_AlphaOnly,      //只显示透明图(不显示其它特殊图标)
        eSAM_AlphaNo,        //不显示透明图(只显示其它特殊图标)
        eSAM_AlphaPerc,      //按概率显示透明图(显示包括透明图的特殊图标)
    }

    /*由上往下滚动
     * image
     * 1、每列image总数比每列可见image数多2个
     * 2、用到锚点坐标
     * 3、多套精灵资源(同时只能显示一套，可以切换)
     * 4、可以选择是否某列只显示透明图或者指定图片
     */
    //如5*3滚动
    //0  3  6  9   12
    //1  4  7  10  13
    //2  5  8  11  14
    public class ItemCtrl_CR_MulRes_Spe
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
        public delegate int CallBackEvn_2(int iCol);
        protected CallBackEvn_2 AShowNumBackEvn;      //火球数字

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

        private int m_iShowResId = 0;         //显示哪套资源
        private List<Sprite[]> m_list_SprItemRes;                  //需要被加载的Item图片
        private Sprite[] m_ary_SprItemBGs = new Sprite[5];          //ItemBG
        private Dictionary<int, List<Image>> m_dic_ListImage;      //image
        private Vector2[] m_ary_VecStartAncPos;                    //image的原始锚点坐标

       // public int m_SpItemResIndex = 0;
        public int m_iBet = 1;//压分倍数
        public int m_GameTypeCurIndex = 0;    //当前游戏类型index 
        
        //public int m_iItem_WILD = 8;            //百搭1
        //public int m_iItem_FREE = 10;           //免费
        public int m_iItem_JackpotBegain = 11;         //彩金开始
        public int m_iItem_FireBegain = 0;//10;            //火球开始
        public int m_iItem_FireEnd = 5;//115;
        public int m_iItem_NornalEnd = 8;

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
                    {
                        //m_dic_ListImage[i][j].sprite = m_list_SprItemRes[m_iShowResId][m_arys_iItemResult[i, m_arys_iItemResult.GetLength(1) - j]];
                        m_dic_ListImage[i][j].transform.GetChild(1).gameObject.SetActive(false);
                        if (m_arys_iItemResult[i, m_arys_iItemResult.GetLength(1) - j] < 8)
                        {
                            m_dic_ListImage[i][j].transform.GetChild(0).GetComponent<Image>().sprite = m_list_SprItemRes[m_iShowResId][m_arys_iItemResult[i, m_arys_iItemResult.GetLength(1) - j]];
                        }
                        //if (m_arys_iItemResult[i, m_arys_iItemResult.GetLength(1) - j] >= m_iItem_FREE /*&& m_arys_iItemResult[i, m_arys_iItemResult.GetLength(1) - j] < m_iItem_WILD*/)
                        //{
                        //    //火球元素
                        
                        
                        if (isFireNum(m_arys_iItemResult[i, m_arys_iItemResult.GetLength(1) - j]))
                            //if (m_arys_iItemResult[i, m_arys_iItemResult.GetLength(1) - j] == 16 || m_arys_iItemResult[i, m_arys_iItemResult.GetLength(1) - j] == 17)
                            m_dic_ListImage[i][j].transform.GetChild(0).GetComponent<Image>().sprite = m_list_SprItemRes[m_iShowResId][m_arys_iItemResult[i, m_arys_iItemResult.GetLength(1) - j]];
                        else
                            m_dic_ListImage[i][j].transform.GetChild(0).GetComponent<Image>().sprite = m_list_SprItemRes[m_iShowResId][10];
                      //  m_arys_ImageItemsNum0[i][j].ShowNum6(GetFireNumStr(m_arys_iItemResult[i, m_arys_iItemResult.GetLength(1) - j]));                       
                        m_dic_ListImage[i][j].transform.GetChild(1).gameObject.SetActive(true);

                        //else if (m_arys_iItemResult[i, m_arys_iItemResult.GetLength(1) - j] >= m_iItem_WILD && m_arys_iItemResult[i, m_arys_iItemResult.GetLength(1) - j] < m_iItem_FREE)
                        //{
                        //    m_dic_ListImage[i][j].sprite = m_list_SprItemRes[m_iShowResId][13];
                        //    m_dic_ListImage[i][j].transform.GetChild(0).GetComponent<Image>().sprite = m_list_SprItemRes[m_iShowResId][m_arys_iItemResult[i, m_arys_iItemResult.GetLength(1) - j]];                            
                        //} 
                        //else //普通元素
                        //    {
                        //        //m_dic_ListImage[i][j].transform.GetChild(0).gameObject.SetActive(false);
                        //        //m_dic_ListImage[i][j].transform.GetChild(1).gameObject.SetActive(false);
                        //    }
                    }
                    else
                    {
                        int iR = UnityEngine.Random.Range(0,6);
                        //if (iR < 8)
                        //{
                        //    if (iR < 4 || iR >= 8)
                        //        m_dic_ListImage[i][j].sprite = m_list_SprItemRes[m_iShowResId][13];
                        //    else
                        //        m_dic_ListImage[i][j].sprite = m_ary_SprItemBGs[iR-4];
                        //    m_dic_ListImage[i][j].transform.GetChild(0).GetComponent<Image>().sprite = m_list_SprItemRes[m_iShowResId][iR];
                        //}
                        //else
                        {
                            //m_dic_ListImage[i][j].sprite = m_list_SprItemRes[m_iShowResId][13];                         
                           
                            m_dic_ListImage[i][j].transform.GetChild(0).GetComponent<Image>().sprite = m_list_SprItemRes[m_iShowResId][iR];

                           // m_arys_ImageItemsNum0[i][j].ShowNum6(GetFireNumStr(iR));
                            m_dic_ListImage[i][j].transform.GetChild(1).gameObject.SetActive(true);
                        }
                    }
                    //m_arys_ImageItemsNum0[i].RemoveAt(0);
                    //m_arys_ImageItemsNum0[i].Add(showNum);
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
                Debug.LogWarning("[ItemCtrl_CR]-> SetAllItemResult() 设置结果失败");
                return;
            }
            // StringBuilder sb = new StringBuilder();

            for (int i = 0; i < arys_iResult.GetLength(0); i++)
            {
                for (int j = 0; j < arys_iResult.GetLength(1); j++)
                {
                    m_arys_iItemResult[i, j] = arys_iResult[i, j];

                    //sb.Append(" i="+i+",j="+j+","+m_arys_iItemResult[i, j]);
                    //Debug.LogError("SetAllItemResult==i" + i + "," + m_arys_iItemResult[i, j]);
                }
            }
            // Debug.LogError("SetAllItemResult==" + sb.ToString());

            // Debug.LogError("SetAllItemResult=="+ arys_iResult.GetLength(0)+","+ arys_iResult.GetLength(1));
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
            if (col < 0 || col >= m_arys_iItemResult.GetLength(0) || row < 0 || row >= m_arys_iItemResult.GetLength(1) || iResult < 0 || iResult >= m_list_SprItemRes[m_iShowResId].Length)
            {
                Debug.LogWarning("[ItemCtrl_CR]-> SetOneResultImage() 传入参数有误！col：" + col + "    row" + row + "  iResult：" + iResult);
                return;
            }
            else
            {
                m_arys_iItemResult[col, row] = iResult;
                m_dic_ListImage[col][m_dic_ListImage[col].Count - 2 - row].sprite = m_list_SprItemRes[m_iShowResId][m_arys_iItemResult[col, row]];
                Debug.Log("设置一个Item结果:  " + m_dic_ListImage[col][m_dic_ListImage[col].Count - 2 - row].sprite.name);
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
                Debug.LogWarning("[ItemCtrl_CR]-> SetAColItemStop() 失败！m_iTransCtrl= " + m_iTransCtrl + "   col：" + col);
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
                Debug.LogWarning("[ItemCtrl_CR]-> Get_m_ary_bIsCanStop() 传入参数有误：col：" + col);
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
                Debug.LogWarning("[ItemCtrl_CR]-> Get_m_ary_bIsStoped() 传入参数有误：col：" + col);
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
                Debug.LogWarning("[ItemCtrl_CR_Double]-> GetImage() 传入参数有误！col：" + col + "    row" + row);
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
        /// 获取或修改显示哪套资源
        /// </summary>
        public int M_iShowResId
        {
            get { return m_iShowResId; }
            set
            {
                if (value >= 0 && value < m_list_SprItemRes.Count)
                {
                    m_iShowResId = value;
                    ShowInitResultSpr();
                }
            }
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
                m_dic_ListImage[col][i].rectTransform.anchoredPosition -= new Vector2(0, fSpeed);
            }
            Image imageTemp = m_dic_ListImage[col][0];
            if (imageTemp.rectTransform.anchoredPosition.y <= m_ary_VecStartAncPos[0].y - m_fImageHeight)
            {
                if (m_ary_iShowResultIdTemp[col] >= m_arys_iItemResult.GetLength(1))
                {
                    m_ary_bIsCanShowParabola[col] = true;
                    bIsCanStop = false;                          //加这句是防止外面设置错误
                }

                if (bIsCanStop)
                {
                   // Debug.LogError("m_ary_eShowAlphaMode[col] : " + m_ary_eShowAlphaMode[col]);
                    switch (m_ary_eShowAlphaMode[col])
                    {
                        case eShowAlphaMode.eSAM_None:
                        case eShowAlphaMode.eSAM_NowReResult:
                        case eShowAlphaMode.eSAM_AlphaNo:

                            int icon = m_arys_iItemResult[col, m_arys_iItemResult.GetLength(1) - 1 - m_ary_iShowResultIdTemp[col]];
                            imageTemp.transform.GetChild(1).gameObject.SetActive(false);            //重置数字显示
                            if (this.isFireNum(icon))
                            {
                                //imageTemp.sprite = m_list_SprItemRes[m_iShowResId][13];
                                //showNum.ShowNum6(this.GetFireNumStr(icon));
                                imageTemp.transform.GetChild(0).GetComponent<Image>().sprite = m_list_SprItemRes[m_iShowResId][icon];
                                imageTemp.transform.GetChild(1).gameObject.SetActive(true);
                            }
                            //else if (this.isJackPotNum(icon))
                            //{
                            //    //彩金元素
                            //    imageTemp.transform.GetChild(0).gameObject.SetActive(true);
                            //    if (imageTemp.transform.GetChild(0).childCount == 1)
                            //    {
                            //        imageTemp.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
                            //        imageTemp.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = m_list_SprItemRes[m_iShowResId][m_arys_iItemResult[col, m_arys_iItemResult.GetLength(1) - 1 - m_ary_iShowResultIdTemp[col]]];
                            //    }
                            //    else
                            //    {
                            //        //GameObject img = GameObject.Instantiate(imageTemp.transform.gameObject, imageTemp.transform.GetChild(0));
                            //        //img.GetComponent<Image>().sprite = m_list_SprItemRes[m_iShowResId][m_arys_iItemResult[col, m_arys_iItemResult.GetLength(1) - 1 - m_ary_iShowResultIdTemp[col]]];
                            //    }
                            //}
                            else
                            {
                                Debug.LogError("走这里?");
                                //if (icon < 4 || icon >= 8)
                                //    imageTemp.sprite = m_list_SprItemRes[m_iShowResId][13];
                                //else
                                //    imageTemp.sprite = m_ary_SprItemBGs[icon - 4];
                                imageTemp.transform.GetChild(0).GetComponent<Image>().sprite = m_list_SprItemRes[m_iShowResId][icon];
                            }
                            break;
                        case eShowAlphaMode.eSAM_AlphaOnly:
                            Debug.Log("eShowAlphaMode.eSAM_AlphaOnly==");
                            imageTemp.sprite = m_list_SprItemRes[m_iShowResId][m_list_SprItemRes[m_iShowResId].Length - 1];
                            break;
                        case eShowAlphaMode.eSAM_AlphaPerc:
                            Debug.Log("eShowAlphaMode.eSAM_AlphaPerc==");
                            {
                                int iSpeItem = GetTheSpeItem(col, m_arys_iItemResult.GetLength(1) - 1 - m_ary_iShowResultIdTemp[col]);
                                if (iSpeItem == -1)
                                    imageTemp.sprite = m_list_SprItemRes[m_iShowResId][m_list_SprItemRes[m_iShowResId].Length - 1];
                                else
                                    imageTemp.sprite = m_list_SprItemRes[m_iShowResId][iSpeItem];
                            }
                            break;
                    }
                    m_ary_iShowResultIdTemp[col]++;
                }
                else
                {
                    switch (m_ary_eShowAlphaMode[col])
                    {
                        case eShowAlphaMode.eSAM_None:
                            
                            int icon = this.GetRandomItemSpIndex();
                            imageTemp.transform.GetChild(1).gameObject.SetActive(false);            //重置数字显示
                            if (this.isFireNum(icon))
                            {
                                //imageTemp.sprite = m_list_SprItemRes[m_iShowResId][13];
                                //showNum.ShowNum6(this.GetFireNumStr(icon));
                                imageTemp.transform.GetChild(0).GetComponent<Image>().sprite = m_list_SprItemRes[m_iShowResId][icon];
                                imageTemp.transform.GetChild(1).gameObject.SetActive(true);
                            }
                            //else if (this.isJackPotNum(icon))
                            //{
                            //    //彩金元素
                            //    imageTemp.transform.GetChild(0).gameObject.SetActive(true);
                            //    if (imageTemp.transform.GetChild(0).childCount == 1)
                            //    {
                            //        imageTemp.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
                            //        imageTemp.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = m_list_SprItemRes[m_iShowResId][m_arys_iItemResult[col, m_arys_iItemResult.GetLength(1) - 1 - m_ary_iShowResultIdTemp[col]]];
                            //    }
                            //    else
                            //    {
                            //        //GameObject img = GameObject.Instantiate(imageTemp.transform.gameObject, imageTemp.transform.GetChild(0));
                            //        //img.GetComponent<Image>().sprite = m_list_SprItemRes[m_iShowResId][m_arys_iItemResult[col, m_arys_iItemResult.GetLength(1) - 1 - m_ary_iShowResultIdTemp[col]]];
                            //    }
                            //}
                            else 
                            {
                                Debug.LogError("走这里？");
                                //if (icon < 4 || icon >= 8)
                                //    imageTemp.sprite = m_list_SprItemRes[m_iShowResId][13];
                                //else
                                //    imageTemp.sprite = m_ary_SprItemBGs[icon-4];
                                imageTemp.transform.GetChild(0).GetComponent<Image>().sprite = m_list_SprItemRes[m_iShowResId][icon];
                            }                            
                            
                            break;
                        case eShowAlphaMode.eSAM_NowReResult:
                            //if (m_ary_iSpeShowItem != null && m_ary_iSpeShowItem.Length > 0)
                            imageTemp.sprite = m_list_SprItemRes[m_iShowResId][UnityEngine.Random.Range(0, m_ary_iShowResultIdTemp.Length - 1)];
                            break;
                        case eShowAlphaMode.eSAM_AlphaOnly:
                            if (m_ary_iSpeShowItem != null && m_ary_iSpeShowItem.Length > 0)
                                imageTemp.sprite = m_list_SprItemRes[m_iShowResId][m_list_SprItemRes[m_iShowResId].Length - 1];
                            break;
                        case eShowAlphaMode.eSAM_AlphaNo:
                            {                                
                                if (m_ary_iSpeShowItem != null && m_ary_iSpeShowItem.Length > 0)
                                {
                                    int iPerc = UnityEngine.Random.Range(0, 100);
                                    if (iPerc <= m_iShowSpeItemPerc)
                                    {
                                        int iSpe = UnityEngine.Random.Range(0, 12);
                                        if (iSpe < 9)
                                        {
                                            imageTemp.sprite = m_list_SprItemRes[m_iShowResId][m_ary_iSpeShowItem[iSpe]];
                                            imageTemp.transform.GetChild(0).gameObject.SetActive(false);
                                        }
                                        else
                                        {
                                            imageTemp.sprite = m_list_SprItemRes[m_iShowResId][iSpe + 100];
                                            imageTemp.transform.GetChild(0).gameObject.SetActive(true);
                                            if (iSpe == 10 || iSpe == 9)
                                            {
                                                imageTemp.transform.GetChild(0).gameObject.SetActive(false);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //imageTemp.sprite = m_list_SprItemRes[m_iShowResId][UnityEngine.Random.Range(0, m_list_SprItemRes[m_iShowResId].Length - 1)];
                                        int iSpe = UnityEngine.Random.Range(0, 12);
                                        if (iSpe < 9)
                                        {
                                            imageTemp.sprite = m_list_SprItemRes[m_iShowResId][m_ary_iSpeShowItem[iSpe]];
                                            imageTemp.transform.GetChild(0).gameObject.SetActive(false);
                                        }
                                        else
                                        {
                                            imageTemp.sprite = m_list_SprItemRes[m_iShowResId][iSpe + 100];
                                            imageTemp.transform.GetChild(0).gameObject.SetActive(true);
                                            if (iSpe == 10 || iSpe == 9)
                                            {                                            
                                                imageTemp.transform.GetChild(0).gameObject.SetActive(false);
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case eShowAlphaMode.eSAM_AlphaPerc:
                            {
                                if (m_ary_iSpeShowItem != null && m_ary_iSpeShowItem.Length > 0)
                                {
                                    int iSpe = 0;
                                    int iPerc = UnityEngine.Random.Range(0, 100);
                                    if (iPerc <= m_iShowAlphaPerc)
                                        iSpe = m_ary_iSpeShowItem.Length - 1;
                                    else
                                        iSpe = UnityEngine.Random.Range(0, m_ary_iSpeShowItem.Length - 1);
                                    imageTemp.sprite = m_list_SprItemRes[m_iShowResId][m_ary_iSpeShowItem[iSpe]];
                                }
                            }
                            break;
                    }
                }
                float fAdjustment = imageTemp.rectTransform.anchoredPosition.y - m_ary_VecStartAncPos[0].y + m_fImageHeight;
                imageTemp.rectTransform.anchoredPosition = new Vector2(m_ary_VecStartAncPos[m_ary_VecStartAncPos.Length - 1].x, m_ary_VecStartAncPos[m_ary_VecStartAncPos.Length - 1].y + fAdjustment);
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

        #region he_add_20180911 由于特殊需求而添加

        private eShowAlphaMode[] m_ary_eShowAlphaMode;   //显示透明图模式
        private int[] m_ary_iSpeShowItem;                //特殊显示Item（最后一张为透明图）
        private int m_iShowAlphaPerc = 0;                //显示透明图概率
        private int m_iShowSpeItemPerc = 0;              //显示特殊图标概率(不包括透明图)

        /// <summary>
        /// 设置需要特殊显示的item
        /// </summary>
        /// <param name="ary_iSpeShowItem">特殊显示item（最后一张为透明图）</param>
        /// <param name="fShowAlphaPerc">显示透明图概率,0-1(只显示特殊图标（包括透明图）)</param>
        public void SetSpeItem(int[] ary_iSpeShowItem, float fShowAlphaPerc = 0.5f)
        {
            m_ary_iSpeShowItem = new int[ary_iSpeShowItem.Length];
            for (int i = 0; i < m_ary_iSpeShowItem.Length; i++)
            {
                m_ary_iSpeShowItem[i] = ary_iSpeShowItem[i];
            }
            fShowAlphaPerc = Mathf.Clamp(fShowAlphaPerc, 0, 1);
            m_iShowAlphaPerc = (int)(fShowAlphaPerc * 100);
        }

        /// <summary>
        /// 设置是否激活特殊显示功能
        /// </summary>
        /// <param name="iCol">列</param>
        /// <param name="eShowAlphaM">显示模式</param>
        /// <param name="fShowSpeItemPerc">显示特殊图标概率(不包括透明图),可以显示非特殊图标</param>
        public void SetSpeEnabAct(int iCol, eShowAlphaMode eShowAlphaM, float fShowSpeItemPerc = 0.5f)
        {
            if (iCol < 0 || iCol >= m_ary_eShowAlphaMode.Length)
                return;
            m_ary_eShowAlphaMode[iCol] = eShowAlphaM;
            switch (m_ary_eShowAlphaMode[iCol])
            {
                case eShowAlphaMode.eSAM_None:
                    break;
                case eShowAlphaMode.eSAM_NowReResult:
                    Debug.Log("eShowAlphaMode.eSAM_NowReResult==");
                    for (int i = 0; i < m_dic_ListImage[iCol].Count; i++)
                    {
                        if (i > 0 && i < m_dic_ListImage[iCol].Count - 1)
                        {
                            m_dic_ListImage[iCol][i].sprite = m_list_SprItemRes[m_iShowResId][m_arys_iItemResult[iCol, m_arys_iItemResult.GetLength(1) - i]];
                        }
                        else
                        {
                            m_dic_ListImage[iCol][i].sprite = m_list_SprItemRes[m_iShowResId][UnityEngine.Random.Range(0, m_list_SprItemRes[m_iShowResId].Length - 1)];
                        }
                    }
                    m_ary_eShowAlphaMode[iCol] = eShowAlphaMode.eSAM_None;
                    break;
                case eShowAlphaMode.eSAM_AlphaOnly:
                    Debug.Log("eShowAlphaMode.eSAM_AlphaOnly==");
                    for (int i = 0; i < m_dic_ListImage[iCol].Count; i++)
                    {
                        m_dic_ListImage[iCol][i].sprite = m_list_SprItemRes[m_iShowResId][m_list_SprItemRes[m_iShowResId].Length - 1];
                    }
                    break;
                case eShowAlphaMode.eSAM_AlphaNo:
                    fShowSpeItemPerc = Mathf.Clamp(fShowSpeItemPerc, 0, 1);
                    m_iShowSpeItemPerc = (int)(fShowSpeItemPerc * 100);
                    break;
                case eShowAlphaMode.eSAM_AlphaPerc:
                    break;
            }
        }

        /// <summary>
        /// 判断是否是特殊item，-1不是
        /// </summary>
        private int GetTheSpeItem(int iCol, int iRow)
        {
            int iSpeItem = -1;
            if (iCol < 0 || iCol >= m_arys_iItemResult.GetLength(0) || iRow < 0 || iRow >= m_arys_iItemResult.GetLength(1))
                return iSpeItem;
            int iResult = m_arys_iItemResult[iCol, iRow];
            for (int i = 0; i < m_ary_iSpeShowItem.Length - 1; i++)
            {
                if (m_ary_iSpeShowItem[i] == iResult)
                {
                    iSpeItem = iResult;
                    break;
                }
            }
            return iSpeItem;
        }

        #endregion

        private int GetRandomItemSpIndex()
        {
            int index = UnityEngine.Random.Range(0, 6);
            return index;
        }

        private bool isFireNum(int index)
        {
            if (index >= m_iItem_FireBegain && index <= m_iItem_FireEnd)
            {
                return true;
            }
            return false;
        }

        private int  GetFireNumStr(int index)
        {
            //if (this.AShowNumBackEvn != null)
            //{
                int num = this.AShowNumBackEvn(index);
                //Debug.LogError("======0=="+ index+"         "+ num);
                return num;
            //}
            //else
            //{
            //    //Debug.LogError("======1");
            //   return this.GetFireNumStr_Normal(index);
            //}
        }

        //int[] score = new int[] { 20,10,5,30,80,25};
        //private int GetFireNumStr_Normal(int index)
        //{
        //    int num = 0;
        //    if (this.isFireNum(index))
        //    {
        //        num = score[index];
        //    }
        //    Debug.LogError("GetFireNumStr_Normal======index:" + index + ",num:"+ num);
        //    return num;
        //}

        //private bool isJackPotNum(int index)
        //{
        //    if(index >= this.m_iItem_JackpotBegain && index< (this.m_iItem_JackpotBegain+4))
        //    {
        //        return true;
        //    }

        //    return false;
        //}
        

}
}
