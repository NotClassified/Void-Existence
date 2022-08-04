using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.UI;
using TMPro;
using System;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public float timeScale = 100;
    public static float time;
    public int levelnum;
    #region BACKGROUND ROCKS
    public GameObject rockParent;
    public GameObject rockPref;
    public Vector3 maxRock;
    public Vector3 minRock; 
    #endregion
    #region PLAYERS
    public GameObject player;
    public GameObject enemy1;
    public GameObject enemy2;
    public float initialSpeedPlayer;
    public float initialSpeedEnemy;
    public bool playerPunchedByEnemy;
    [SerializeField]
    GameObject playerPref;
    [SerializeField]
    GameObject enemyPref;
    [SerializeField]
    bool[] actionsEnemy;
    [SerializeField]
    int actionIndex1;
    [SerializeField]
    int actionIndex2;
    [SerializeField]
    float spawnOffsetForEnemy2;
    #endregion
    #region ENVIRONMENT
    public GameObject prefWallMarker;
    [SerializeField]
    Light worldLight;
    [SerializeField]
    float delaySpawnEnemy;
    [SerializeField]
    Transform environment;
    int envChildCountStart; //initail children count off environment
    [SerializeField]
    GameObject[] levels;
    [SerializeField]
    Transform playerSpawn;
    [SerializeField]
    Transform enemySpawn;
    [SerializeField]
    GameObject endPointPortal;
    #endregion
    #region GOAL //g-Goal
    public GameObject gCam;
    public int countGoal;
    [SerializeField]
    Transform gCanvas;
    [SerializeField]
    TextMeshProUGUI gText;
    [SerializeField]
    VideoPlayer gLoopVideoPlayer;
    [SerializeField]
    VideoPlayer gInitialVideoPlayer;
    [SerializeField]
    GameObject gInitialVideoDisplay;
    [SerializeField]
    GameObject gSkipPopUp;
    [SerializeField]
    string[] gMessagesStart;
    [SerializeField]
    string[] gMessagesEnd;
    [SerializeField]
    VideoClip[] gLoopClips;
    [SerializeField]
    VideoClip[] gInitialClips;
    #endregion
    #region COUNTER //c-Counter s-Slider
    [SerializeField]
    GameObject cCanvas;
    [SerializeField]
    TextMeshProUGUI cTextHeader;
    [SerializeField]
    Slider cSlider;
    [SerializeField]
    float csVelocity;
    float csTarget;
    float csValue = 0;
    float csDelta;
    private int count = 0;
    [SerializeField]
    string[] cHeaders;
    #endregion
    #region TUTORAIL //tut-Tutorial
    public int tutNumber;
    [SerializeField]
    float tutTransDelay;
    [SerializeField]
    GameObject tutCanvas;
    [SerializeField]
    GameObject tutExtra;
    [SerializeField]
    Image tutKey;
    [SerializeField]
    TextMeshProUGUI tutText;
    [SerializeField]
    Color32 tutInputColorON;
    [SerializeField]
    Color32 tutInputColorOFF;
    [SerializeField]
    GameObject tutGreyTint;
    [SerializeField]
    GameObject tutSkip;
    bool tutSkipAbility;
    [SerializeField]
    Sprite[] keys;
    [SerializeField]
    string[] tutMessages;
    #endregion
    #region PROGRESS
    [SerializeField] GameObject progressCanvas;
    [SerializeField] GameObject progressBar;
    [SerializeField] Slider progressPlayer;
    [SerializeField] Slider progressEnemy;
    [SerializeField] Transform portalStart;
    [SerializeField] Transform portalEnd;
    [SerializeField] float portalTriggerOffsetZ;
    float portalTriggerOffsetY = -2.3f;
    bool progressPerfectTutorial = true;
    bool progressTutorialRespawn = false;
    float progressDistance;
    Vector3 progressStartPosition;
    #endregion
    #region ENEMY ACTION MARKERS //ea-enemy action
    [SerializeField]
    GameObject eaMarker;
    [SerializeField]
    Transform eaParent;
    #endregion

    public static bool showHUD = true;
    public static float brightness = 1f;
    bool gameover = false;
    public static bool levelFinished;
    [SerializeField] float finishLevelDelay;
    bool loadingScene = false;
    public int mode = 0; //0-Tutorial 1-Level 2-No Enemies

    void Start()
    {
        //for (int i = 0; i < 2000; i++)
        //{
        //    GameObject rock = Instantiate(rockPref, rockParent.transform);
        //    rock.transform.position = new Vector3(Random.Range(minRock.x, maxRock.x), Random.Range(minRock.y, maxRock.y), 
        //                                          Random.Range(minRock.z, maxRock.z));
        //    rock.transform.eulerAngles = new Vector3(Random.Range(-360, 360), Random.Range(-360, 360), Random.Range(-360, 360));
        //    float sizeOfRock = Random.Range(.1f, 1.5f);
        //    rock.transform.localScale = new Vector3(sizeOfRock, sizeOfRock, sizeOfRock);
        //}

        Time.timeScale = timeScale / 100;
        //if (mode == 0)
        //{
        //    Instantiate(playerPref);
        //}
        //else if(mode == 1)
        //{
        //    GameObject player = Instantiate(playerPref);
        //    player.transform.SetPositionAndRotation(playerSpawn.position, playerSpawn.rotation);
        //    //Instantiate(enemyPref);
        //}
        //else
        //{

        //}
        envChildCountStart = environment.childCount;

        //if (gCanvas != null)
        //    gCam.SetActive(true);
        //if (numTutorial != -1)
        //    print("Tutorials skipped: " + (numTutorial + 1) + "\n Please set numTutorial = -1");
        //if (tutCanvas != null)
        //    NextTutorial();
        //else
        //    StartLevel();

        ChangeBrightness();
        levelFinished = false;
        if (mode == 0)
            StartTutorial();
        else if (mode == 1)
        {
            if (showHUD)
                progressCanvas.SetActive(true);
            else
                progressCanvas.SetActive(false);

            progressDistance = Vector3.Distance(portalStart.position, portalEnd.position);
            progressStartPosition = portalStart.position;

            progressPerfectTutorial = false;
            StartLevel();
        }
    }
    private void Update()
    {
        #region COUNTER SLIDER VALUE SMOOTHING
        if (mode == 0)
        {
            if (count < 0) //Player's foward speed = target value
                csTarget = 0;
            else if (count > countGoal)
                csTarget = countGoal;
            else
                csTarget = count;
            //MAKE THE VALUE GRADUALLY CHANGE TO TARGET:
            csDelta = csTarget - csValue; //difference between target value and actual value (slider's value)
            csDelta *= Time.deltaTime * csVelocity; //make actual value gradually change
            csValue += csDelta; //increase actual value closer to target value
            cSlider.value = csValue; //correlate slider values
        }
        
        #endregion
        #region PROGRESS BAR VALUE
        if (mode == 1 && progressCanvas.activeSelf)
        {
            progressPlayer.value = Vector3.Distance(progressStartPosition, player.transform.position) / progressDistance;

            if (enemy1.transform.position.z < progressStartPosition.z)
            {
                if (enemy2 != null)
                    progressEnemy.value = Vector3.Distance(progressStartPosition, enemy2.transform.position) / progressDistance;
                else if (enemy1 != null)
                    progressEnemy.value = Vector3.Distance(progressStartPosition, enemy1.transform.position) / progressDistance;
            }
            else
                progressEnemy.value = 0;
        }
        #endregion
        #region PROGRESS CHECK
        if (player.transform.position.z < portalEnd.position.z + portalTriggerOffsetZ)
        {
            if (!levelFinished)
            {
                if (mode == 0)
                {
                    if (progressPerfectTutorial && count >= countGoal) //if player completed tutorial perfectly
                        levelFinished = true;
                    else
                    {
                        progressPerfectTutorial = false;
                        progressTutorialRespawn = true;
                    }
                }
                else if (mode == 1)
                    LevelFinished(levelnum);
            }

            if (player.transform.position.y < portalEnd.position.y + portalTriggerOffsetY)
            {
                if (progressPerfectTutorial) //completed tutorial perfectly, load next level
                    LoadNextLevel();

                else if (progressTutorialRespawn) //didn't complete tutorial, respawn player
                    player.transform.SetPositionAndRotation(playerSpawn.position, playerSpawn.rotation);

                else if (levelFinished) //completed level, go back to main menu
                    ResetGame();

                else
                    Debug.LogError("didn't register whether player completed level or tutorial or did tutorial perfectly");
            }
        }
        #endregion
        //if (Input.GetKeyDown(KeyCode.Space) && !gInitialVideoDisplay.activeSelf && gCam.activeSelf)
        //    StartPlayerTutorial();
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (mode == 1)
                ReloadLevel();
            else if (mode == 0 && tutSkipAbility)
                LoadNextLevel();
        }
    }

    #region OLD TUTORIAL METHODS
    //public void StartPlayerTutorial()
    //{
    //    gameover = false;
    //    playerPunchedByEnemy = false;
    //    if (player != null) //if players exist, destroy for new players
    //        Destroy(player);
    //    if (enemy1 != null) //if enemy exists, destroy
    //        Destroy(enemy1);
    //    gCam.SetActive(false); //toggle off goal camera

    //    //TOGGLE UI VISIBILITY:
    //    foreach (Transform gChild in gCanvas)
    //        gChild.gameObject.SetActive(false);
    //    foreach (Transform cChild in cCanvas)
    //        cChild.gameObject.SetActive(true);
    //    foreach (Transform tutChild in tutCanvas)
    //        tutChild.gameObject.SetActive(true);
    //    if (tutNumber != 2)
    //        tutExtra.SetActive(false);

    //    //SPAWNING PLAYERS AND MAKING LEVEL:
    //    player = Instantiate(playerPref);
    //    player.transform.SetPositionAndRotation(playerSpawn.position, playerSpawn.rotation);
    //    //Vector3 spawnRot = new Vector3(playerSpawn.rotation.x, playerSpawn.rotation.y, playerSpawn.rotation.z);
    //    //player.transform.rotation = Quaternion.Euler(spawnRot);
    //    //player.transform.SetPositionAndRotation(playerSpawn.position, playerSpawn.rotation);
    //    if (environment.childCount > envChildCountStart)
    //        Destroy(environment.GetChild(0).gameObject);
    //    Instantiate(levels[tutNumber], environment).transform.SetAsFirstSibling();
    //    //player.GetComponent<PlayerTrick>().lAlways = true;
    //    //this.CallDelay(StartEnemy, delaySpawnEnemy);
    //    if (tutNumber == 3)
    //    {
    //        enemy1 = Instantiate(enemyPref);
    //        enemy1.GetComponent<EnemyTrick>().enemyNum = 1;
    //        enemy1.transform.SetPositionAndRotation(enemySpawn.position, enemySpawn.rotation);
    //    }
    //}

    //public void NextTutorial() //when player completes goal, go to next tutorial
    //{
    //    tutNumber++; //increase index of tutorial
    //    int numTut = tutNumber; //get index of tutorial
    //    if (numTut >= tutMessages.Length) //if there are no more tutorials and goals left, end tutorial
    //    {
    //        player.GetComponent<PlayerUI>().TextFeedback("Tutorial Finished!", 0);
    //        this.CallDelay(ResetGame, 3f);
    //        GameProgress.LevelComplete(0);
    //        return;
    //    }

    //    if (numTut == 3)
    //        countGoal = 1;

    //    //SET UI FOR NEXT GOAL:
    //    StartCoroutine(WaitForFirstTutorial(numTut)); //comment this line for faster testing*/ print("this line commented");
    //    gText.text = gMessagesStart[numTut].Replace("/newline/", "\n") + countGoal.ToString() + gMessagesEnd[numTut]; //goal
    //    //SET UI FOR NEXT COUNTER:
    //    cTextHeader.text = cHeaders[numTut]; //action for counter
    //    //SET UI FOR NEXT TUTORIAL:
    //    tutKey.sprite = keys[numTut]; //key input
    //    tutText.text = tutMessages[numTut]; //action for key

    //    //TOGGLE UI VISIBILITY:
    //    foreach (Transform gChild in gCanvas)
    //        gChild.gameObject.SetActive(false);
    //    foreach (Transform cChild in cCanvas)
    //        cChild.gameObject.SetActive(false);
    //    foreach (Transform tutChild in tutCanvas)
    //        tutChild.gameObject.SetActive(false);
    //    //RESET COUNTER:
    //    csValue = 0f;
    //    count = 0;

    //    gCam.SetActive(true); //toggle on goal camera
    //    //destroy players, level, and world space UI, if they exist:
    //    if (player != null)
    //        Destroy(player);
    //    if (environment.childCount > envChildCountStart)
    //        Destroy(environment.GetChild(0).gameObject);
    //    if (environment.Find("Wall Markers").childCount != 0)
    //    {
    //        foreach (Transform wallMarkerChild in environment.Find("Wall Markers"))
    //            Destroy(wallMarkerChild.gameObject);
    //    }

    //    //FOR FASTER TESTING: //Also comment out line 217 (this line -33)
    //    //foreach (Transform gChild in gCanvas) //toggle UI for goal canvas
    //    //    gChild.gameObject.SetActive(true);
    //    //gInitialVideoDisplay.SetActive(false); //toggle display of tutorial
    //    //gSkipPopUp.SetActive(false); //toggle skip text
    //    //gLoopVideoPlayer.clip = gLoopClips[numTut]; //2nd part of video tutorial plays
    //    //StartPlayerTutorial();
    //    //print("Faster Testing Block is Uncommented");
    //}
    //IEnumerator WaitForFirstTutorial(int numTut)
    //{
    //    gInitialVideoPlayer.clip = gInitialClips[numTut]; //beginning video tutorial plays
    //    gInitialVideoDisplay.SetActive(true); //toggle display of tutorial
    //    gSkipPopUp.SetActive(false); //toggle skip text
    //    yield return new WaitForSeconds((float)gInitialClips[numTut].length);

    //    gInitialVideoPlayer.clip = gInitialClips[numTut]; //play again
    //    gSkipPopUp.SetActive(true); //toggle skip text
    //    float timeOut = 0;

    //    while (!Input.GetKeyDown(KeyCode.Space) && timeOut < (float)gInitialClips[numTut].length * 2) //play video twice unless player skips
    //    {
    //        timeOut += Time.deltaTime;
    //        yield return null;
    //    }
    //    foreach (Transform gChild in gCanvas) //toggle UI for goal canvas
    //        gChild.gameObject.SetActive(true);
    //    gInitialVideoDisplay.SetActive(false); //toggle display of tutorial
    //    gSkipPopUp.SetActive(false); //toggle skip text
    //    gLoopVideoPlayer.clip = gLoopClips[numTut]; //2nd part of video tutorial plays
    //}

    //public IEnumerator LastCountWaitForLand() //before tutorial finishes for jumping, make sure player lands
    //{
    //    PlayerTrick pt_ = player.GetComponent<PlayerTrick>();
    //    while (!pt_.isLanding)
    //        yield return null;
    //    yield return new WaitForFixedUpdate();
    //    if (pt_.AnimCheck(0, "Land1"))
    //        this.CallDelay(NextTutorial, tutTransDelay);
    //}

    //public void IncreaseCounter() //when player follows goal, increase count
    //{
    //    if (cCanvas != null)
    //    {
    //        count++;
    //        if (count >= countGoal) //goal completed
    //        {
    //            if (numTutorial != 2)
    //                this.CallDelay(NextTutorial, tutTransDelay);
    //            else
    //                StartCoroutine(LastCountWaitForLand());
    //        }
    //    }
    //}
    //public void DecreaseCounter() //when player doesn't land after jump
    //{
    //    if (cCanvas != null && numTutorial == 2 && count > 0)
    //        count--;
    //}

    //public int GetCount() => count;
    #endregion

    #region TUTORIAL METHODS
    void StartTutorial()
    {
        player = Instantiate(playerPref);
        player.transform.SetPositionAndRotation(playerSpawn.position, playerSpawn.rotation);

        if (showHUD)
        {
            tutCanvas.SetActive(true);
            cCanvas.SetActive(true);
        }
        else
        {
            tutCanvas.SetActive(false);
            cCanvas.SetActive(false);
        }

        if (GameProgress.tutorialLastCompleted >= tutNumber) //if player has completed tutorial before, let player skip tutorial
        {
            tutSkipAbility = true;
            if (showHUD)
                tutSkip.SetActive(true);
        }
        else
        {
            tutSkipAbility = false;
            tutSkip.SetActive(false);
        }
    }

    public void LightUpInputText(bool lightUpText)
    {
        if (lightUpText)
        {
            tutKey.color = tutInputColorON; //light up key
            tutKey.GetComponent<Animator>().SetBool("Input", true); //play input animation
        }
        else
        {
            tutKey.color = tutInputColorOFF; //unlight key
            tutKey.GetComponent<Animator>().SetBool("Input", false); //reset input animation to default
            tutGreyTint.SetActive(false); //toggle off grey tint ui
        }
    }
    public bool GetInputTextLit() => tutKey.color == tutInputColorON;

    public void FreezeTutorial()
    {
        tutGreyTint.SetActive(true); //toggle on grey tint ui
        Time.timeScale = 0; //freeze tutorial to wait for player to input correct action
    }

    public void IncreaseCounter() //when player follows goal, increase count
    {
        if (cCanvas != null)
        {
            count++;
            if (count >= countGoal) //if goal completed, end tutorial
            {
                GameProgress.TutorialComplete(tutNumber); //tutorial progress

                if (progressPerfectTutorial) //player finished tutorial perfectly
                {
                    player.GetComponent<PlayerUI>().TextFeedback("Tutorial Finished Perfectly!", 0);
                }
                else
                {
                    player.GetComponent<PlayerUI>().TextFeedback("Tutorial Finished!", 0);
                    this.CallDelay(LoadNextLevel, finishLevelDelay); //load next level
                }
            }
        }
    }

    public int GetCount() => count;

    void LoadNextLevel()
    {
        if (!loadingScene)
        {
            loadingScene = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    #endregion

    public void ChangeBrightness() => worldLight.intensity = brightness;
    public void ShowHUD()
    {
        showHUD = !showHUD;
        if (mode == 0)
        {
            tutCanvas.SetActive(showHUD);
            cCanvas.SetActive(showHUD);
        }
        else if (mode == 1)
        {
            progressCanvas.gameObject.SetActive(showHUD);
        }
    }

    void StartLevel()
    {
        //GameObject level_ = Instantiate(levels[0], environment);
        //level_.transform.SetAsFirstSibling();
        //level_.AddComponent<LevelPrep>();

        player = Instantiate(playerPref);
        if(mode == 1)
        {
            enemy1 = Instantiate(enemyPref);
            enemy1.GetComponent<EnemyTrick>().enemyNum = 1;
            //enemy2 = Instantiate(enemyPref);
            //enemy2.GetComponent<EnemyTrick>().enemyNum = 2;
        }
        StartCoroutine(Spawn());

    }

    public void LevelFinished(int level_)
    {
        player.GetComponent<PlayerUI>().TextFeedback("Level Finished!", 0);
        if (enemy2 != null)
            enemy2.GetComponent<EnemyTrick>().StartEnemyStopRunningRoutine();
        else if (enemy1 != null)
            enemy1.GetComponent<EnemyTrick>().StartEnemyStopRunningRoutine();
        levelFinished = true;
        GameProgress.LevelComplete(level_);
    }

    public bool GetEnemyAction(int enemyNum)
    {
        if (actionsEnemy.Length > actionIndex1) //if there is an action left
        {
            if (enemyNum == 2 && actionsEnemy.Length > actionIndex2) //if actionindex is being given to left enemy
            {
                //MAKE ENEMY ACTION MARKERS FOR LEVEL:
                //GameObject marker_ = Instantiate(eaMarker, enemy2.transform.position, eaMarker.transform.rotation); //create
                //marker_.transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text = actionIndex2.ToString(); //change text to index of action
                //marker_.transform.SetParent(eaParent); //make child of a parent

                return actionsEnemy[actionIndex2++];
            }
            return actionsEnemy[actionIndex1++]; //give actionIndex, then increase
        }
        return true;
    }

    public IEnumerator Spawn()
    {
        if(tutCanvas != null)
        {
            player.transform.SetPositionAndRotation(playerSpawn.position, playerSpawn.rotation); //reset position
            PlayerTrick pt_ = player.GetComponent<PlayerTrick>();
            yield return new WaitForEndOfFrame();
            while (pt_.AnimCheck(0, "Exit")) //wait until player resets
                yield return null;
            player.GetComponent<PlayerMovement>().velocityZ = initialSpeedPlayer; //reset speed
            if(enemy1 != null)
            {
                Destroy(enemy1);
                enemy1 = Instantiate(enemyPref);
                enemy1.GetComponent<EnemyTrick>().enemyNum = 1;
                enemy1.transform.SetPositionAndRotation(enemySpawn.position, enemySpawn.rotation);
            }
        }
        else
        {
            if (mode == 1)
            {
                while (enemy1.GetComponent<EnemyMovement>().gm == null) //wait until this enemy's script can access this script
                    yield return null;
            }
            //RESET POSITIONS AND INDEX OF ALL PLAYERS:
            player.transform.SetPositionAndRotation(playerSpawn.position, playerSpawn.rotation);

            if(mode == 1)
            {
                enemy1.GetComponent<EnemyMovement>().ResetPlayer();
                enemy1.transform.SetPositionAndRotation(enemySpawn.position, enemySpawn.rotation);
                actionIndex1 = 0;

                //enemy2.GetComponent<EnemyMovement>().ResetPlayer();
                //enemy2.transform.SetPositionAndRotation(enemySpawn.GetChild(0).position, enemySpawn.rotation);
                //actionIndex2 = 0;

                EnemyTrick et1_ = enemy1.GetComponent<EnemyTrick>();
                //EnemyTrick et2_ = enemy2.GetComponent<EnemyTrick>();
                PlayerTrick pt_ = player.GetComponent<PlayerTrick>();
                yield return new WaitForEndOfFrame();
                while (et1_.AnimCheck(0, "Exit") || /*et2_.AnimCheck(0, "Exit") ||*/ pt_.AnimCheck(0, "Exit")) //wait until all players reset
                    yield return null;
                //RESET SPEED FOR ALL PLAYERS:
                enemy1.GetComponent<EnemyMovement>().velocityZ = initialSpeedEnemy;
                //enemy2.GetComponent<EnemyMovement>().velocityZ = initialSpeedEnemy;
                player.GetComponent<PlayerMovement>().velocityZ = initialSpeedPlayer;
            }

            gameover = false;
            Time.timeScale = timeScale / 100;
            //UsefulShortcuts.ClearConsole();
        }
    }

    public IEnumerator GameOver()
    {
        gameover = true;
        while (!playerPunchedByEnemy)
            yield return null;

        if(player.GetComponent<PlayerTrick>().dodgedEnemy)
        {
            gameover = false;
            playerPunchedByEnemy = false;

            if (tutCanvas != null)
                IncreaseCounter();

            enemy2 = Instantiate(enemyPref);
            enemy2.GetComponent<EnemyTrick>().enemyNum = 2;
            enemy2.transform.position = new Vector3(2, enemy1.transform.position.y, enemy1.transform.position.z + spawnOffsetForEnemy2);
            enemy2.transform.eulerAngles = new Vector3(0, -90);
        }
        else
        {
            if (tutCanvas == null) //if not in tutorial, show player "Game Over"
                player.GetComponent<PlayerUI>().TextFeedback("Game Over", 5);
            yield return new WaitForSeconds(1f);
            //if (tutCanvas != null) //if in tutorial, don't reload scene
            //    StartPlayerTutorial();
            //else
                ReloadLevel();
        }
    }
    public bool IsGameOver() => gameover;
    public void ReloadLevel()
    {
        if (mode == 0 && GameProgress.tutorialLastCompleted >= tutNumber) //if player has already completed tutorial
        {
            ResetGame();
        }
        else if (mode == 1 && !player.GetComponent<PlayerUI>().GetFeedbackText().Equals("Level Finished!")) //level
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
    public void ResetGame() => SceneManager.LoadScene(0);
}

public static class MonoBehavoirExtension
{
    public static void CallDelay(this MonoBehaviour mono, Action method, float delay) => mono.StartCoroutine(CallDelayRoutine(method, delay));

    static IEnumerator CallDelayRoutine(Action method, float delay)
    {
        yield return new WaitForSeconds(delay);
        method();
    }

}

//public static class UsefulShortcuts
//{
//    public static void ClearConsole()
//    {
//        var assembly = Assembly.GetAssembly(typeof(SceneView));
//        var type = assembly.GetType("UnityEditor.LogEntries");
//        var method = type.GetMethod("Clear");
//        method.Invoke(new object(), null);
//    }
//}