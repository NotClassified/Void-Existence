using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndPoint : MonoBehaviour
{
    [SerializeField]
    GameManager gm;
    [SerializeField]
    int levelIndex;
    public bool perfectTutorial = true;

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        if (levelIndex != 0)
    //            gm.LevelFinished(levelIndex);
    //        else if(perfectTutorial && gm.GetCount() == 5)
    //        {
    //            gm.FinishTutorialPerfectly();
    //        }
    //        else
    //        {
    //            perfectTutorial = false; //player didn't do tutorial perfectly
    //            gm.ReSpawnPlayerTutorial(); //reset player back to spawn
    //        }
    //    }
    //}
}
