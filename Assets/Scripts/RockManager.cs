using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockManager : MonoBehaviour
{
    //public bool startCheckingRenderers;
    //public int rockIndex;
    //public int rockRenderersLength;
    //public Renderer[] rockRenderers;
    public bool[] rockHasRendered;
    public bool destroyChildren;

    void Start()
    {
        //get "rockRenderersLength" amount of renderers from rocks starting from "rockIndex"
        //rockRenderers = new Renderer[rockRenderersLength];
        //int rockRendererIndex = 0;
        //while (rockRendererIndex < rockRenderersLength && rockIndex < transform.childCount)
        //{
        //    rockRenderers[rockRendererIndex++] = transform.GetChild(rockIndex++).GetComponent<Renderer>();
        //}

        //rockHasRendered = new bool[rockRenderers.Length];

        rockHasRendered = new bool[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.AddComponent<RockVisibleCheck>().childIndex = i;
        }
    }

    public void rockHasBecameVisible(int child)
    {
        rockHasRendered[child] = true;
    }

    private void Update()
    {
        if (destroyChildren)
        {
            destroyChildren = false;
            for (int i = 0; i < rockHasRendered.Length; i++)
            {
                if (rockHasRendered[i])
                    Destroy(transform.GetChild(i).gameObject);
            }
        }
    }
}
