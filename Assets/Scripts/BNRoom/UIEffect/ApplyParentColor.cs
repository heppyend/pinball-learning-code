using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class ApplyParentColor : MonoBehaviour
{
    void Update()
    {
        if (transform.GetComponent<SkeletonGraphic>() != null)
        {
            transform.GetComponent<SkeletonGraphic>().color = transform.parent.GetComponent<Image>().color;
        }
        else if (transform.GetComponent<Image>() != null)
        {
            transform.GetComponent<Image>().color = transform.parent.GetComponent<Image>().color;
        }
    }
}
