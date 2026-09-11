using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UITools
{
    #region 自动取消RatcastTarget
    //Image
    [MenuItem("GameObject/UI/MyImage")]
    static void CreatImage()
    {
        var activeTrans = Selection.activeTransform;
        if (activeTrans)
        {
            if (activeTrans.GetComponentInParent<Canvas>())
            {
                GameObject creatObj = new GameObject("Image", typeof(Image));
                creatObj.GetComponent<Image>().raycastTarget = false;
                creatObj.transform.SetParent(activeTrans, false);
                Selection.activeGameObject = creatObj;
            }
        }
    }

    [MenuItem("Component/UI/MyImage")]
    static void AddComponentImage()
    {
        var activeObj = Selection.activeGameObject;
        if (activeObj)
        {
            if (activeObj.GetComponent<RectTransform>())
            {
                var image = activeObj.AddComponent<Image>();
                image.raycastTarget = false;
            }
        }
    }

    //Text
    [MenuItem("GameObject/UI/MyText")]
    static void CreatText()
    {
        var activeTrans = Selection.activeTransform;
        if (activeTrans)
        {
            if (activeTrans.GetComponentInParent<Canvas>())
            {
                GameObject creatObj = new GameObject("Text", typeof(Text));
                creatObj.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 30);
                var text = creatObj.GetComponent<Text>();
                text.supportRichText = false;
                text.raycastTarget = false;
                creatObj.transform.SetParent(activeTrans, false);
                Selection.activeGameObject = creatObj;
            }
        }
    }

    [MenuItem("Component/UI/MyText")]
    static void AddComponentText()
    {
        var activeObj = Selection.activeGameObject;
        if (activeObj)
        {
            if (activeObj.GetComponent<RectTransform>())
            {
                var text = activeObj.AddComponent<Text>();
                text.raycastTarget = false;
                text.supportRichText = false;
            }
        }
    }
    #endregion

    [MenuItem("Tools/通用工具/切换物体显隐状态 %q")]
    static void SetObjActive()
    {
        GameObject[] selectObjs = Selection.gameObjects;
        int objCtn = selectObjs.Length;
        for (int i = 0; i < objCtn; i++)
        {
            bool isAcitve = selectObjs[i].activeSelf;
            selectObjs[i].SetActive(!isAcitve);
        }
    }
}
