using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LevelPrep : MonoBehaviour
{
    #region MARKERS //m-Markers
    List<Vector3> mPositions;
    GameObject wallMarker;
    Transform parWallMarkers;
    #endregion

    void Start()
    {
        //load and find references:
        wallMarker = FindObjectOfType<GameManager>().prefWallMarker;
        parWallMarkers = transform.parent.Find("Wall Markers");

        mPositions = new List<Vector3>(); //construct list of positions for wall markers instantiate method
        foreach (Transform child in transform) //traverse all children of this object
        {
            if (child.name.Substring(0, 4).Equals("Cube"))
                child.gameObject.layer = 6; //platforms
            else if (child.name.Substring(0, 4).Equals("Wall"))
            {
                child.gameObject.layer = 3; //walls
                mPositions.Add(child.GetChild(0).position); //add position to lists
            }
        }
        if(wallMarker != null)
        {
            foreach (Vector3 pos in mPositions)
            {
                Instantiate(wallMarker, pos, wallMarker.transform.rotation, parWallMarkers);
            }
        }
    }
}
