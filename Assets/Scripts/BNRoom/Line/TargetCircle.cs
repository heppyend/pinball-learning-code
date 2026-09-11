using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetCircle : MonoBehaviour
{
    private LineRenderer lineRender;
    public LineRenderer LineRender
    {
        get
        {
            if (ReferenceEquals(lineRender,null))
            {
                lineRender = transform.parent.GetComponent<LineRenderer>();
            }
            return lineRender;
        }
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<MeshRenderer>().enabled = LineRender.enabled;
        transform.localPosition = LineRender.GetPosition(LineRender.positionCount - 1);
    }
}
