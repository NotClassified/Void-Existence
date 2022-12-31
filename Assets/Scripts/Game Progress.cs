using UnityEngine;

public class GameProgress
{
    public static int levelLastCompleted = 0; //default is 0
    public static int tutorialLastCompleted = 0; //default is 0
    public static float[] levelTimeRecords = new float[10]; //index 1 = level 1


    public static void SaveGameProgress()
    {
        SaveManager.SaveData();
    }
    public static void LoadGameProgress()
    {
        SavedData progressData = SaveManager.LoadData();
        if (progressData != null)
        {
            levelLastCompleted = progressData.levelCompleted;
            tutorialLastCompleted = progressData.tutorialCompleted;
            levelTimeRecords = progressData.levelTimeRecords;
        }
    }
    public static void ResetGameProgress()
    {
        levelLastCompleted = 0; 
        tutorialLastCompleted = 0;
        levelTimeRecords = new float[10];
        SaveGameProgress();
        Debug.Log("File Reset");
    }

    //if the completed level was the lastest level, increase levelCompleted index
    public static void LevelComplete(int level)
    {
        levelLastCompleted = levelLastCompleted == level - 1 ? level : levelLastCompleted;
    }
    //if the completed tutorial was the lastest tutorial, increase tutorialCompleted index
    public static void TutorialComplete(int tutorial)
    {
        tutorialLastCompleted = tutorialLastCompleted == tutorial - 1 ? tutorial : tutorialLastCompleted;
    }

    //only set and return true if the time record is faster than best record
    public static bool SetTimeRecord(int level, float time)
    {
        if (levelTimeRecords[level] > time || levelTimeRecords[level] == 0)
        {
            levelTimeRecords[level] = time;
            return true;
        }
        return false;
    }
}
