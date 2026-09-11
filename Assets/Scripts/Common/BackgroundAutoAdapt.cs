using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundAutoAdapt : MonoBehaviour
{
    private bool isAdapt = false;

    private void Awake()
    {
        MessageCenter.Instance.AddListener(MsgType.CLIENT_INNERGAME_OPENCLOSE, onOpenOrCloseInnerGame);
    }

    private void OnDestroy()
    {
        MessageCenter.Instance.RemoveListener(MsgType.CLIENT_INNERGAME_OPENCLOSE, onOpenOrCloseInnerGame);
    }

    private void Update()
    {
        resize();
        if (!isAdapt && !ReferenceEquals(GameController.Instance, null))
        {
            isAdapt = true;
            resize();
        }
    }

    private void onOpenOrCloseInnerGame(Message msg)
    {
        resize();
    }

    private void resize()
    {
        var rect = GameController.Instance.transform as RectTransform;
        var width = rect.rect.width;
        var height = rect.rect.height;
        if (transform.localScale != new Vector3(width / 2160.0f, height / 3840.0f, 1))
        {
            transform.localScale = new Vector3(width / 2610.0f, height / 3840.0f, 1);
        }
    }
}
