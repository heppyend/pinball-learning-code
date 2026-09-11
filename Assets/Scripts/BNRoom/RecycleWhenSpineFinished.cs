using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;

public class RecycleWhenSpineFinished : MonoBehaviour
{
    private TrackEntry _trackEntry;

    public enum SpineType
    {
        UI,
        World
    }
    public SpineType spineType;
    
    private SkeletonAnimation _skeletonAnimation;
    public SkeletonAnimation SkeletonAnimation
    {
        get
        {
            _skeletonAnimation = GetComponent<SkeletonAnimation>();
            return _skeletonAnimation;
        }
    }
    
    private SkeletonGraphic _skeletonGraphic;
    public SkeletonGraphic SkeletonGraphic
    {
        get
        {
            _skeletonGraphic = GetComponent<SkeletonGraphic>();
            return _skeletonGraphic;
        }
    }
    
    public string animationName;
    
    void OnEnable()
    {
        if (spineType == SpineType.UI)
        {
            _trackEntry = SkeletonGraphic.AnimationState.SetAnimation(0, animationName, false);
        }
        if (spineType == SpineType.World)
        {
            _trackEntry = SkeletonAnimation.AnimationState.SetAnimation(0, animationName, false);
        }
        
        _trackEntry.Complete += UnSpawnSelf;
    }

    void UnSpawnSelf(TrackEntry trackEntry)
    {
        _trackEntry.Complete -= UnSpawnSelf;
        if (animationName == "Ultimate")
        {
            BNPlayerController.Instance.isPlayingUltimate = false;
        }
        if (gameObject.activeSelf)
        {
            ObjectPoolManager.Instance.Unspawn(gameObject);
        }
    }
}
