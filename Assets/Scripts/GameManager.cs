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
    public bool rocksInstantiate = false;
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
    public GameObject enemyExtra;
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
    Transform environment;
    int envChildCountStart; //initail children count off environment
    [SerializeField]
    GameObject[] levels;
    [SerializeField]
    Transform playerSpawn;
    [SerializeField]
    Transform enemySpawn;
    [SerializeField]
    Transform[] extraEnemySpawns;
    [SerializeField]
    float extraEnemySpawnTrigOffset;
    int extraEnemyIndex;
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
    public int tutNumber; //1 = land, 2 = walljump, 3 = jump, 4 = dodge
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
    [SerializeField] 
    GameObject tutskipButtonAndroid;
    bool tutSkipAbility;
    [SerializeField]
    Sprite[] keys;
    [SerializeField]
    string[] tutMessages;
    #endregion
    #region PROGRESS
    private GameObject progressCanvas;
    private Slider progressPlayer;
    private Slider progressEnemy;
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
    #region TIMER
    GameObject timerCanvasPrefab;
    Transform timerCanvas;
    TextMeshProUGUI timerText;
    TextMeshProUGUI timerMilisecondText;
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
    int enemyLevel;
    int playerLevel;
    [SerializeField] GameObject audioManagerPrefab;
    [SerializeField] float distancePlayerToEnemyAllowed = 15f;
    public static bool levelFinished;
    [SerializeField] float finishLevelDelay;
    bool loadingScene = false;
    public static bool playAllLevels;
    public int mode = 0; //0-Tutorial 1-Level 2-No Enemies
    public static bool developerMode = false;
    float timeMeasure;


    void Start()
    {
        #region SPAWN ROCKS FOR ENVIRONMENT
        if (rocksInstantiate)
        {
            Debug.Log("creating Rocks");
            for (int i = 0; i < rocksAmount; i++)
            {
                GameObject rock = Instantiate(rockPref, rockParent.transform);
                rock.transform.position = new Vector3(Random.Range(minRock.x, maxRock.x), Random.Range(minRock.y, maxRock.y),
                                                      Random.Range(minRock.z, maxRock.z));
                rock.transform.eulerAngles = new Vector3(Random.Range(-360, 360), Random.Range(-360, 360), Random.Range(-360, 360));
                float sizeOfRock = Random.Range(.1f, 1.5f);
                rock.transform.localScale = new Vector3(sizeOfRock, sizeOfRock, sizeOfRock);
            }
        }
        #endregion

        //get progress bar references
        progressCanvas = GameObject.FindGameObjectWithTag("ProgressBar");
        if (progressCanvas != null)
        {
            progressPlayer = progressCanvas.transform.GetChild(0).GetChild(0).GetComponent<Slider>();
            progressEnemy = progressCanvas.transform.GetChild(0).GetChild(1).GetComponent<Slider>();
        }

        if (timeScale != 100) Debug.Log("timeScale not set to default (100)");
        Time.timeScale = timeScale / 100;

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

            //make and start timer
            StartCoroutine(StartTimer());

            StartLevel();
        }
        else
            StartLevel();

        //replace the prompts for android build
        if(player.GetComponent<PlayerMovement>().androidBuild && tutKey != null)
        {
            switch (tutNumber)
            {
                case 1: //landing tutorial
                    tutKey.sprite = Resources.Load<Sprite>("finger swipe down");
                    break;
                case 2: //wall jumping tutorial
                    tutKey.sprite = Resources.Load<Sprite>("finger swipe up");
                    break;
                case 3: //jumping tutorial
                    //resize image to square ratio and move closer to text
                    tutKey.transform.localPosition = new Vector2(-50f, tutKey.transform.localPosition.y);
                    tutKey.transform.localScale = new Vector2(tutKey.transform.localScale.x / 2, tutKey.transform.localScale.y);
                    tutKey.sprite = Resources.Load<Sprite>("finger swipe up");
                    tutExtraKey.sprite = Resources.Load<Sprite>("finger swipe down");
                    break;
                case 4: //dodge tutorial
                    tutKey.sprite = Resources.Load<Sprite>("finger swipe down");
                    break;
            }
            //switch out pc prompt with button for skipping tutorial
            if (tutSkip.activeSelf)
            {
                tutskipButtonAndroid.SetActive(true);
                tutSkip.transform.GetChild(0).gameObject.SetActive(false);
            }
        }
    }
    private void Update()
    {
        #region DEVELOPER MODE
        //toggle developer mode
        //if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.D))
        //{
        //    developerMode = !developerMode;
        //    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        //}
        if (Input.GetKeyDown(KeyCode.E)) //stop timer tool
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
        if (Input.GetKeyDown(KeyCode.P) && developerMode)
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
                    {
                        levelFinished = true;
                        GameProgress.SaveGameProgress();
                    }
                    else
                    {
                        progressPerfectTutorial = false;
                        progressTutorialRespawn = true;
                        GameProgress.SaveGameProgress();
                    }
                }
                else if (mode == 1)
                    LevelFinished(levelnum);
            }

            if (player.transform.position.y < portalEnd.position.y + portalTriggerOffsetY)
            {
                if (progressPerfectTutorial || tutNumber == 4) //completed tutorial perfectly or completed dodge tutorial, load next level
                {
                    LoadNextLevel();
                    return;
                }
                else if (progressTutorialRespawn) //didn't complete tutorial, respawn player
                    player.transform.SetPositionAndRotation(playerSpawn.position, playerSpawn.rotation);

                else if (levelFinished) //completed level, go back to main menu
                    ResetGame();

                else
                    Debug.LogError("didn't register whether player completed level or tutorial or did tutorial perfectly");

                AudioManager.instance.PlaySound("portal");
                if (AudioManager.instance.airAudioIsPlaying)
                    AudioManager.instance.StopSound("air");
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

            //if (mode == 0) ReloadLevel(); print("developer restart enabled");
        }
        #endregion
        #region EXTRA ENEMY SPAWN
        if(mode == 1 && levelnum == 5 && extraEnemySpawns.Length > extraEnemyIndex &&
           player.transform.position.z < extraEnemySpawns[extraEnemyIndex].position.z + extraEnemySpawnTrigOffset)
        {
            enemyExtra = Instantiate(enemyPref);
            float posY = extraEnemySpawns[extraEnemyIndex].position.y;
            float posZ = extraEnemySpawns[extraEnemyIndex].position.z;
            if (enemy2 != null) //enemy2 exists
            {
                enemyExtra.GetComponent<EnemyTrick>().enemyNum = 3; 
                enemyExtra.transform.position = new Vector3(-2, posY, posZ); //enemy on right side
            }
            else //enemy2 does not exists
            {
                enemyExtra.GetComponent<EnemyTrick>().enemyNum = 4; 
                enemyExtra.transform.position = new Vector3(2, posY, posZ); //enemy on left side
            }
            enemyExtra.transform.eulerAngles = new Vector3(0, -90);
            extraEnemyIndex++;
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

        if (AudioManager.instance.airAudioIsPlaying)
            AudioManager.instance.StopSound("air");
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

    public void LoadNextLevel()
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

    IEnumerator StartTimer()
    {
        timerCanvasPrefab = Resources.Load<GameObject>("Canvas_Timer");
        timerCanvas = Instantiate(timerCanvasPrefab).transform;
        timerText = timerCanvas.GetChild(0).GetComponent<TextMeshProUGUI>();
        timerMilisecondText = timerCanvas.GetChild(1).GetComponent<TextMeshProUGUI>();

        float time = 0;
        while (!levelFinished) //stop timer when player finishes level
        {
            time += Time.deltaTime;
            timerText.text = TimeObject.ConvertTimeMINSEC(time) + ":";
            timerMilisecondText.text = TimeObject.Miliseconds2Digit(time);
            yield return null;
        }
        if (GameProgress.SetTimeRecord(levelnum, time))
        {
            timerCanvas.GetChild(2).gameObject.SetActive(true);
            GameProgress.SaveGameProgress();
        }
    }
    void StartLevel()
    {
        if (AudioManager.instance == null)
            Instantiate(audioManagerPrefab);
        AudioManager.instance.PlaySound("portal");
        if (AudioManager.instance.airAudioIsPlaying)
            AudioManager.instance.StopSound("air");
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
            enemy2.GetComponent<EnemyTrick>().EnemyStopRunning();
        else if (enemy1 != null)
            enemy1.GetComponent<EnemyTrick>().EnemyStopRunning();

        levelFinished = true;

        GameProgress.LevelComplete(level_);
        GameProgress.SaveGameProgress();
    }

    public bool GetEnemyAction()
    {
        if (actionsEnemy.Length > actionIndex) //if there is an action left
        {
            #region MAKE ENEMY ACTION MARKERS FOR LEVEL
            if (eaStopPlayerForActionMarkers)
            {
                Debug.Log("creating enemy action markers");
                if (eaDestroyChildren && eaParent.childCount != 0)
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
            #endregion

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
        Time.timeScale = timeScale / 100;
        //UsefulShortcuts.ClearConsole();
    }


    public IEnumerator GameOver()
    {
        player.GetComponent<PlayerUI>().LightUpRestartPrompt(); //let player know that they can restart level

        float distancePlayerAndEnemy = 0;
        while (!playerPunchedByEnemy && distancePlayerAndEnemy < distancePlayerToEnemyAllowed)
        {
            if(enemy2 != null) //if enemy2 exists, use enemy2 position
                distancePlayerAndEnemy = player.transform.position.z - enemy2.transform.position.z;
            else
                distancePlayerAndEnemy = player.transform.position.z - enemy1.transform.position.z;
            yield return null;
        }

        if (distancePlayerAndEnemy >= distancePlayerToEnemyAllowed) //player fell too far behind
        {
            player.GetComponent<PlayerUI>().TextFeedback("Game Over: Too Far Behind", 5);
            yield return new WaitForSeconds(2f);
            ReloadLevel();
        }
        else if (player.GetComponent<PlayerTrick>().dodgedEnemy)
        {
            playerPunchedByEnemy = false;
            if (player.GetComponent<PlayerTrick>().extraEnemyIsPunching) //extra enemy was dodged
                this.CallDelay(player.GetComponent<PlayerTrick>().ExtraEnemyDodgeReset, .2f);
            else if (mode == 1)
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