
public class GameProgress
{
    public static int levelLastCompleted = 0; //default is 0
    public static int tutorialLastCompleted = 1; //default is 0

    //if the completed level was the lastest level, increase levelCompleted index
    public static void LevelComplete(int level) => levelLastCompleted = levelLastCompleted == level - 1 ? level + 1 : levelLastCompleted;

    //if the completed tutorial was the lastest tutorial, increase tutorialCompleted index
    public static void TutorialComplete(int tutorial) => tutorialLastCompleted = tutorialLastCompleted == tutorial-1 ? tutorial+1 : tutorialLastCompleted;

}
