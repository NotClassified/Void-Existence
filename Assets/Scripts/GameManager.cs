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
    public int rocksAmount;
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
    int actionIndex;
    [SerializeField]
    float spawnDelayForEnemy2;
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
    GameObject tutCanvas;
    [SerializeField]
    Image tutKey;
    [SerializeField]
    Image tutExtraKey;
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
    Transform progressPlayerPosition;
    Transform progressEnemyPosition;
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
    [SerializeField]
    Material eaCorrectActionColor;
    public bool eaStopPlayerForActionMarkers = false;
    bool eaDestroyChildren = true;
    #endregion

    public static bool showHUD = true;
    public static float brightness = 1f;
    bool gameover = false;
    int enemyLevel;
    int playerLevel;
    [SerializeField] float distancePlayerToEnemyAllowed = 15f;
    public static bool levelFinished;
    [SerializeField] float finishLevelDelay;
    bool loadingScene = false;
    public int mode = 0; //0-Tutorial 1-Level 2-No Enemies
    public static bool developerMode = true;
    float timeMeasure;


    void Start()
    {
        //SPAWN ROCKS FOR ENVIRONMENT
        //for (int i = 0; i < rocksAmount; i++)
        //{
        //    GameObject rock = Instantiate(rockPref, rockParent.transform);
        //    rock.transform.position = new Vector3(Random.Range(minRock.x, maxRock.x), Random.Range(minRock.y, maxRock.y),
        //                                          Random.Range(minRock.z, maxRock.z));
        //    rock.transform.eulerAngles = new Vector3(Random.Range(-360, 360), Random.Range(-360, 360), Random.Range(-360, 360));
        //    float sizeOfRock = Random.Range(.1f, 1.5f);
        //    rock.transform.localScale = new Vector3(sizeOfRock, sizeOfRock, sizeOfRock);
        //}
        if (timeScale != 100) Debug.Log("timeScale not set to default (100)");
        Time.timeScale = timeScale / 100;
        envChildCountStart = environment.childCount;

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
        else
            StartLevel();
    }
    private void Update()
    {
        #region DEVELOPER MODE
        //toggle developer mode
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.D))
        {
            developerMode = !developerMode;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        if (Input.GetKeyDown(KeyCode.E)) //stop timer
        {
            if(timeMeasure == 0)
            {
                timeMeasure = Time.time;
            }
            else
            {
                Debug.LogWarning(timeMeasure = Time.time - timeMeasure);
                timeMeasure = 0;
            }
        }
        //stop player
        if (eaStopPlayerForActionMarkers && developerMode)
        {
            player.GetComponent<PlayerMovement>().velocityZ = 0;
            player.transform.SetPositionAndRotation(playerSpawn.position, playerSpawn.rotation);
        }
        //toggle stop player
        if (Input.GetKeyDown(KeyCode.P))
            eaStopPlayerForActionMarkers = !eaStopPlayerForActionMarkers;
        #endregion
        #region COUNTER SLIDER VALUE SMOOTHING
        if (mode == 0 && tutNumber != 4)
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
        if (mode == 1 && progressCanvas.activeSelf && progressEnemyPosition != null) //in the level mode, and the progress bar is showing
        {
            //percentage of the level traveled by the player
            progressPlayer.value = Vector3.Distance(progressStartPosition, progressPlayerPosition.position) / progressDistance;

            if (enemy1.transform.position.z < progressStartPosition.z) //if enemy has reached the start of the level
            {
                //percentage of the level traveled by the enemy
                progressEnemy.value = Vector3.Distance(progressStartPosition, progressEnemyPosition.position) / progressDistance;
            }
            else
                progressEnemy.value = 0; //enemy hasn't reached the starting point of the level
        }
        #endregion
        #region PROGRESS CHECK
        if (player != null && player.transform.position.z < portalEnd.position.z + portalTriggerOffsetZ)
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
                if (progressPerfectTutorial || tutNumber == 4) //completed tutorial perfectly or completed dodge tutorial, load next level
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
        #region RESTART/SKIP TUTORIAL
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (mode == 1)
                ReloadLevel();
            else if (mode == 0 && tutSkipAbility)
                LoadNextLevel();

            //if (mode == 0) ReloadLevel(true); print("developer restart enabled");
        } 
        #endregion
    }

    #region TUTORIAL METHODS
    void StartTutorial()
    {
        player = Instantiate(playerPref);
        player.transform.SetPositionAndRotation(playerSpawn.position, playerSpawn.rotation);
        if (tutNumber == 4)
        {
            enemy1 = Instantiate(enemyPref);
            enemy1.GetComponent<EnemyTrick>().enemyNum = 1;
            StartCoroutine(Spawn());
        }

        if (showHUD)
        {
            tutCanvas.SetActive(true);
            if (cCanvas != null)
                cCanvas.SetActive(true);
        }
        else
        {
            tutCanvas.SetActive(false);
            if (cCanvas != null)
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
        //if player is about to enter portal, prevent text from lighting up
        if (lightUpText && !(player.transform.position.z < portalEnd.position.z + portalTriggerOffsetZ))
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
    public void LightUpExtraInputText(bool lightUpText)
    {
        if (lightUpText)
        {
            tutExtraKey.color = tutInputColorON; //light up key
            tutExtraKey.GetComponent<Animator>().SetBool("Input", true); //play input animation
        }
        else
        {
            tutExtraKey.color = tutInputColorOFF; //unlight key
            tutExtraKey.GetComponent<Animator>().SetBool("Input", false); //reset input animation to default
        }
    }
    public bool GetExtraInputTextLit() => tutExtraKey.color == tutInputColorON;

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
            if (cCanvas != null)
                cCanvas.SetActive(showHUD);
        }
        else if (mode == 1)
        {
            progressCanvas.gameObject.SetActive(showHUD);
        }
    }


    void StartLevel()
    {
        player = Instantiate(playerPref);
        if(mode == 1)
        {
            enemy1 = Instantiate(enemyPref);
            enemy1.GetComponent<EnemyTrick>().enemyNum = 1;
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
        if (actionsEnemy.Length > actionIndex) //if there is an action left
        {
            //MAKE ENEMY ACTION MARKERS FOR LEVEL:
            if (eaStopPlayerForActionMarkers)
            {
                Debug.Log("creating enemy action markers");
                if (eaDestroyChildren && eaParent.GetChild(0) != null)
                {
                    eaDestroyChildren = false;
                    foreach (Transform eaChild in eaParent)
                        Destroy(eaChild.gameObject);
                }
                //create marker using enemy postition
                GameObject marker_ = Instantiate(eaMarker, enemy1.transform.position + new Vector3(-2, 0), eaMarker.transform.rotation);
                marker_.transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text = actionIndex.ToString(); //change text to index of action
                if (actionsEnemy[actionIndex])
                    marker_.transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().color = eaCorrectActionColor.color;
                marker_.transform.SetParent(eaParent); //make the action marker a child of the parent
            }

            return actionsEnemy[actionIndex++]; //give actionIndex, then increase
        }
        return true;
    }
    void SpawnEnemy2()
    {
        enemy2 = Instantiate(enemyPref);
        enemy2.GetComponent<EnemyTrick>().enemyNum = 2;
        enemy2.transform.position = new Vector3(2, enemy1.transform.position.y, enemy1.transform.position.z);
        enemy2.transform.eulerAngles = new Vector3(0, -90);

        progressEnemyPosition = enemy2.GetComponent<EnemyMovement>().rootBone; //progress bar checks enemy2 instead of enemy1
    }


    public IEnumerator Spawn()
    {
        if (enemy1 != null)
        {
            while (enemy1.GetComponent<EnemyMovement>().gm == null) //wait until this enemy's script can access this script
                yield return null;
        }

        //RESET POSITIONS AND INDEX OF ALL PLAYERS:
        player.transform.SetPositionAndRotation(playerSpawn.position, playerSpawn.rotation);

        if (enemy1 != null)
        {
            enemy1.GetComponent<EnemyMovement>().ResetPlayer();
            enemy1.transform.SetPositionAndRotation(enemySpawn.position, enemySpawn.rotation);
            if (actionIndex != 0) Debug.Log("enemy action index isn't set to default (0)");
        }

        if(mode == 1)
        {
            EnemyTrick et1_ = enemy1.GetComponent<EnemyTrick>();
            PlayerTrick pt_ = player.GetComponent<PlayerTrick>();
            yield return new WaitForEndOfFrame();
            while (et1_.AnimCheck(0, "Exit") || pt_.AnimCheck(0, "Exit")) //wait until all players reset
                yield return null;
        }
        //RESET SPEED FOR ALL PLAYERS:
        if (enemy1 != null)
            enemy1.GetComponent<EnemyMovement>().velocityZ = initialSpeedEnemy;
        player.GetComponent<PlayerMovement>().velocityZ = initialSpeedPlayer;


        if (mode == 1)
        {
            progressPlayerPosition = player.GetComponent<PlayerMovement>().rootBone;
            progressEnemyPosition = enemy1.GetComponent<EnemyMovement>().rootBone;
        }
        gameover = false;
        Time.timeScale = timeScale / 100;
        //UsefulShortcuts.ClearConsole();
    }


    public IEnumerator GameOver()
    {
        gameover = true;
        float distancePlayerAndEnemy = 0;
        while (!playerPunchedByEnemy && distancePlayerAndEnemy < distancePlayerToEnemyAllowed)
        {
            if(enemy2 != null)
                distancePlayerAndEnemy = player.transform.position.z - enemy2.transform.position.z;
            else
                distancePlayerAndEnemy = player.transform.position.z - enemy1.transform.position.z;
            yield return null;
        }

        if (distancePlayerAndEnemy >= distancePlayerToEnemyAllowed)
        {
            player.GetComponent<PlayerUI>().TextFeedback("Game Over: Too Far Behind", 5);
            yield return new WaitForSeconds(2f);
            ReloadLevel();
        }
        else if (player.GetComponent<PlayerTrick>().dodgedEnemy)
        {
            gameover = false;
            playerPunchedByEnemy = false;
            if (mode == 1) //normal level mode
                this.CallDelay(SpawnEnemy2, spawnDelayForEnemy2);
        }
        else
        {
            if (tutNumber != 4) //if not in dodge tutorial, show player "Game Over"
                player.GetComponent<PlayerUI>().TextFeedback("Game Over", 5);
            StartCoroutine(player.GetComponent<PlayerMovement>().CameraShake());
            yield return new WaitForSeconds(1f);
            ReloadLevel();
        }
    }
    public bool IsGameOver() => gameover;

    public bool IsPlayerAndEnemyOnSameLevel() => playerLevel == enemyLevel;
    public void SetPlayerLevel(int level) => playerLevel = level;
    public void SetEnemyLevel(int level) => enemyLevel = level;

    public void ReloadLevel()
    {
        if (eaStopPlayerForActionMarkers)
        {
            enemy1.transform.SetPositionAndRotation(enemySpawn.position, enemySpawn.rotation);
            eaDestroyChildren = true;
            actionIndex = 0;
        }
        else if (!player.GetComponent<PlayerUI>().GetFeedbackText().Equals("Level Finished!")) //prevent reload when finishing level
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