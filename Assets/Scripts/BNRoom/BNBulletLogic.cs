using System;
using System.Collections;
using System.Collections.Generic;
using BNRoom.Static;
using DG.Tweening;
using LC.Newtonsoft.Json.Linq;
using UnityEngine;

public class BNBulletLogic : MonoBehaviour
{
    [Header("子弹速度")]
    float BulletSpeed = 5f;

    // 子弹伤害
    private int BulletDamage = 10;
    // 攻击者类型系数
    private float attackerTypeFactor;
    // 元素类型
    private int elementType;
    // 暴击伤害
    private int CriticalDamage;
    // 暴击率
    private int CriticalRate;
    // 效果类型
    private int _effectType;
    // 效果参数
    private int _effectParam;

    private float lifeTime = 10f;
    
    private Camera _mainCamera;

    private GameObject _target;

    private int _attacker;
    
    // 是否完成初始化
    private bool isInitialized = false;

    private void OnEnable()
    {
        _mainCamera = Camera.main;
        lifeTime = 10f;
    }
    
    public void Init(int damage, float typeFactor, int element , int criticalDamage, int criticalRate, int effectType,int effectParam,GameObject target,int attacker = 0)
    {
        BulletDamage = damage;
        attackerTypeFactor = typeFactor;
        elementType = element;
        CriticalDamage = criticalDamage;
        CriticalRate = criticalRate;
        _effectType = effectType;
        _effectParam = effectParam;
        _attacker = attacker;
        _target = target;

        isInitialized = true;
        CurveMove();
    }

    private void FixedUpdate()
    {
        transform.localPosition += transform.up * (BulletSpeed * Time.fixedDeltaTime);
        
        lifeTime -= Time.fixedDeltaTime;
        if (lifeTime <= 0)
        {
            if (gameObject.activeSelf)
                ObjectPoolManager.Instance.Unspawn(gameObject);
        }

        if ((_target.GetComponent<Collider2D>().enabled == false || _target == null) && isInitialized)
        {
            if (gameObject.activeSelf)
                ObjectPoolManager.Instance.Unspawn(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Marble"))
        {
            if (collision.gameObject != _target) return;
            collision.transform.GetComponent<BNMarbleLogic>().GetHit(
                BulletDamage,
                attackerTypeFactor,
                elementType,
                CriticalDamage,
                CriticalRate
            );
            ObjectPoolManager.Instance.Spawn(
                ResManager.Instance.LoadPrefab(SysDefines.MARBLEEFFECTS + (EnumElementType)elementType + "Element/" + "Hit"),
                collision.transform.position,
                Quaternion.identity,
                BNGameManager.Instance.EffectRoom
            );
            if (gameObject.activeSelf)
                ObjectPoolManager.Instance.Unspawn(gameObject);
        }
        else if (collision.CompareTag("EnemyMarble"))
        {
            if(_target != null && collision.gameObject != _target) return;
            collision.transform.GetComponent<BNEnemyMarbleLogic>().TakeDamage(
                BulletDamage,
                attackerTypeFactor,
                elementType,
                CriticalDamage,
                CriticalRate,
                _attacker,
                false
            );
            ObjectPoolManager.Instance.Spawn(
                ResManager.Instance.LoadPrefab(SysDefines.MARBLEEFFECTS + (EnumElementType)elementType + "Element/" + "Hit"),
                collision.transform.position,
                Quaternion.identity,
                BNGameManager.Instance.EffectRoom
            );
            if (gameObject.activeSelf)
                ObjectPoolManager.Instance.Unspawn(gameObject);
        }
    }
    
    /// <summary>
    /// 曲线移动
    /// </summary>
    private void CurveMove()
    {
        float duration = 0.6f;
        float arcHeight = 1.5f;
        float padding = 0f;
        
        Vector3 start = transform.position;
        Vector3 end = ClampToScreen(_target.transform.position, padding);
        
        // 计算控制点：起点和终点的中点，然后向上偏移（或向任意方向）
        Vector3 mid = (start + end) / 2;
        // 垂直偏移方向：垂直于 (end - start) 的方向（2D）
        Vector3 dir = (end - start).normalized;
        Vector3 perp = new Vector3(-dir.y, dir.x, 0);
        Vector3 control = mid + perp * arcHeight;

        // 确保控制点也在屏幕内
        control = ClampToScreen(control, padding);

        // 构建路径点
        Vector3[] path = new Vector3[] { start, control, end };

        // 使用 DOPath 曲线移动
        transform.DOPath(path, duration,PathType.CatmullRom)
            .SetEase(Ease.Linear);
    }
    
    Vector3 ClampToScreen(Vector3 worldPos, float padding)
    {
        Vector3 bottomLeft = _mainCamera.ScreenToWorldPoint(new Vector3(0, 0, 0));
        Vector3 topRight = _mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));

        float minX = bottomLeft.x + padding;
        float maxX = topRight.x - padding;
        float minY = bottomLeft.y + padding;
        float maxY = topRight.y - padding;

        worldPos.x = Mathf.Clamp(worldPos.x, minX, maxX);
        worldPos.y = Mathf.Clamp(worldPos.y, minY, maxY);
        worldPos.z = 0;
        return worldPos;
    }
}
