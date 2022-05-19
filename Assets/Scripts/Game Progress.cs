
public class GameProgress
{
    public static int levelCompleted = 1; //default is 0

    //if the level that was completed was the last unlocked level, increase levelCompleted index
    public static void LevelComplete(int level) => levelCompleted = levelCompleted == level ? level + 1 : levelCompleted;

}
