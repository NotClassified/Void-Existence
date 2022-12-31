using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SavedData
{
    public int levelCompleted;
    public int tutorialCompleted;
    public float[] levelTimeRecords;

    public SavedData()
    {
        levelCompleted = GameProgress.levelLastCompleted;
        tutorialCompleted = GameProgress.tutorialLastCompleted;
        levelTimeRecords = GameProgress.levelTimeRecords;
    }
}
