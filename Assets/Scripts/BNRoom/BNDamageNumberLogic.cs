using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class BNDamageNumberLogic : MonoBehaviour
{
    public enum MonsterType
    {
        None,
        Mob,
        Elite,
        LittleBoss,
        LargeBoss
    }
    
    public MonsterType monsterType;

    private float _randomValue;
    
    private TextMeshProUGUI tmp;
    public TextMeshProUGUI TMP
    {
        get
        {
            if (ReferenceEquals(tmp, null))
            {
                tmp = UnityHelper.FindTheChild(gameObject, "TMP").GetComponent<TextMeshProUGUI>();
            }
            return tmp;
        }
    }
    
    private void OnEnable()
    {
        transform.localScale = Vector3.zero;
        TMP.color = Color.clear;
    }

    public void MoveStart(Vector3 position,int type)
    {
        monsterType = (MonsterType)type;
        ModifyTheRandomValue();
        Vector3 offset = position;
        float[] xOffset = { -0.3f,-0.2f,-0.1f,0,0.1f,0.2f, 0.3f };
        offset.x += xOffset[Random.Range(0, xOffset.Length)];
        transform.position = offset;
        offset.y += _randomValue;
        float delay = 0.5f;
        transform.DOScale(Vector3.one, delay);
        transform.DOMove(offset, delay);
        TMP.DOColor(Color.white, delay);
        StartCoroutine(DelayDestroy(delay));
    }
    
    IEnumerator DelayDestroy(float delay)
    {
        yield return new WaitForSeconds(delay + 0.5f);
        float downDelay = 1f;
        TMP.DOColor(Color.clear, downDelay);
        transform.DOMoveY(transform.position.y - 0.2f, downDelay)
            .OnComplete(UnspawnObject);
    }

    private void ModifyTheRandomValue()
    {
        switch (monsterType)
        {
            case MonsterType.Mob:
                _randomValue = Random.Range(0.7f, 1f);
                break;
            case MonsterType.Elite:
                _randomValue = Random.Range(0.9f, 1.2f);
                break;
            case MonsterType.LittleBoss:
                _randomValue = Random.Range(1.2f, 1.5f);
                break;
            case MonsterType.LargeBoss:
                _randomValue = Random.Range(2.6f, 2.9f);
                break;
        }
    }
    
    void UnspawnObject()
    {
        if(gameObject.activeSelf)
            ObjectPoolManager.Instance.Unspawn(gameObject);
    }
}
