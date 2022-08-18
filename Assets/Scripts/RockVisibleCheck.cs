using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockVisibleCheck : MonoBehaviour
{
    public int childIndex;
    public bool visible;
    RockManager parentScript;
    private void Start()
    {
        parentScript = GetComponentInParent<RockManager>();
    }

    private void Update()
    {
        //checkVisibility = parentScript.startCheckingRenderers;
    }

    private void OnBecameVisible()
    {
        parentScript.rockHasBecameVisible(childIndex);
        visible = true;
    }
}
