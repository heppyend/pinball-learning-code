using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BNEffectCollect : MonoBehaviour
{
    public enum EffectType
    {
        DamageNum = 0,
        EnemyBullet = 1,
    }
    
    public EffectType EffectTypeEnum;
    
    public float CollectTime = 5f;
    void OnEnable()
    {
        StartCoroutine(CollectThis());
    } 
    IEnumerator CollectThis()
    {
        yield return new WaitForSeconds(CollectTime);
        //检测枚举类型
        switch (EffectTypeEnum)
        {
            case EffectType.DamageNum:
                transform.DOScale(Vector3.zero, 0.25f)
                    .SetEase(Ease.OutQuart)
                    .OnComplete(() => UnspawnObject() );
                break;
            case EffectType.EnemyBullet:
                UnspawnObject();
                break;
        }
        
    }

    void UnspawnObject()
    {
        if(gameObject.activeSelf)
            ObjectPoolManager.Instance.Unspawn(gameObject);
    }
}
