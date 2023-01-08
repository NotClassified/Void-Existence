using UnityEngine;

public class GameProgress
{
    public static int levelLastCompleted = 0; //default is 0
    public static int tutorialLastCompleted = 0; //default is 0
    ///<summary> index 0 = all levels, index 1 = level 1  </summary>
    public static float[] levelTimeRecords = new float[10];
    public static float tempAllLevelTimeRecord;


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

    ///<summary> if the completed level was the lastest level, increase levelCompleted index </summary>
    public static void LevelComplete(int level)
    {
        levelLastCompleted = levelLastCompleted == level - 1 ? level : levelLastCompleted;
    }
    ///<summary> if the completed tutorial was the lastest tutorial, increase tutorialCompleted index </summary>
    public static void TutorialComplete(int tutorial)
    {
        tutorialLastCompleted = tutorialLastCompleted == tutorial - 1 ? tutorial : tutorialLastCompleted;
    }

    ///<summary> only set and return true if the time record is faster than best record </summary>
    public static bool SetTimeRecord(int level, float time)
    {
        if (levelTimeRecords[level] > time || levelTimeRecords[level] == 0)
        {
            levelTimeRecords[level] = time;
            return true;
        }
        return false;
    }
    ///<summary> add to temporary time unless it's the last level which will only set and return true if faster than best record </summary>
    public static bool SetAllLevelTimeRecord(bool setTemporaryTime, float time, bool resetTempTime)
    {
        if (resetTempTime)
            tempAllLevelTimeRecord = 0;
        if (setTemporaryTime)
        {
            tempAllLevelTimeRecord += time;
        }
        else if(levelTimeRecords[0] > time || levelTimeRecords[0] == 0)
        {
            tempAllLevelTimeRecord += time;
            levelTimeRecords[0] = tempAllLevelTimeRecord;
            return true;
        }
        return false;
    }
}
