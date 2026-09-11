using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncGameObject : MonoBehaviour
{
    public GameObject syncTarget;

    private void Update()
    {
        if (syncTarget)
            transform.position = syncTarget.transform.position;
    }
}
