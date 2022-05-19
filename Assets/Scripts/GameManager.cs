﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.UI;
using TMPro;
using System;

public class GameManager : MonoBehaviour
{
    public float timeScale = 100;
    #region PLAYERS
    private GameObject player;
    private GameObject enemy1;
    private GameObject enemy2;
    public float initialSpeedPlayer;
    public float initialSpeedEnemy;
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
    #endregion
    #region ENVIRONMENT
    public GameObject prefWallMarker;
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
    Transform cCanvas;
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
    public int numTutorial;
    [SerializeField]
    float tutTransDelay;
    [SerializeField]
    Transform tutCanvas;
    [SerializeField]
    GameObject tutExtra;
    [SerializeField]
    Image tutKey;
    [SerializeField]
    TextMeshProUGUI tutText;
    [SerializeField]
    Sprite[] keys;
    [SerializeField]
    string[] tutMessages;
    #endregion
    #region ENEMY ACTION MARKERS //ea-enemy action
    [SerializeField]
    GameObject eaMarker;
    [SerializeField]
    Transform eaParent;
    #endregion

    bool gameover = false;
    public int mode = 0; //0-singleplayer 1-singleplayer+enemy 2-multiplayer

    void Start()
    {
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

        if (gCanvas != null)
            gCam.SetActive(true);
        if (numTutorial != -1)
            print("Tutorials skipped: " + (numTutorial + 1));
        if (tutCanvas != null)
            NextTutorial();
        else
            StartLevel();
    }
    private void Update()
    {
        #region COUNTER SLIDER VALUE SMOOTHING
        if (count < 0) //Player's foward speed = target value
            csTarget = 0;
        else if (count > countGoal)
            csTarget = countGoal;
        else
            csTarget = count;
        //make the value gradually change to target:
        csDelta = csTarget - csValue; //difference between target value and actual value (slider's value)
        csDelta *= Time.deltaTime * csVelocity; //make actual value gradually change
        csValue += csDelta; //increase actual value closer to target value
        cSlider.value = csValue; //correlate slider values
        #endregion
        if (Input.GetKeyDown(KeyCode.Space) && !gInitialVideoDisplay.activeSelf && gCam.activeSelf)
            StartPlayerTutorial();
        if (Input.GetKeyDown(KeyCode.Escape))
            ResetGame();

        if (enemy1 != null)
        {
            if (!gameover && enemy1.transform.position.z + 1 < player.transform.position.z)
            {
                gameover = true;
                player.GetComponent<PlayerMovement>().ResetPlayer();
            }
        }
    }

    #region TUTORIAL METHODS
    public void StartPlayerTutorial()
    {
        if (player != null) //if players exist, destroy for new players
            Destroy(player);
        gCam.SetActive(false); //toggle off goal camera

        //toggle UI visibility:
        foreach (Transform gChild in gCanvas)
            gChild.gameObject.SetActive(false);
        foreach (Transform cChild in cCanvas)
            cChild.gameObject.SetActive(true);
        foreach (Transform tutChild in tutCanvas)
            tutChild.gameObject.SetActive(true);
        if (numTutorial != 2)
            tutExtra.SetActive(false);

        //spawning players and making level:
        player = Instantiate(playerPref);
        player.transform.SetPositionAndRotation(playerSpawn.position, playerSpawn.rotation);
        //Vector3 spawnRot = new Vector3(playerSpawn.rotation.x, playerSpawn.rotation.y, playerSpawn.rotation.z);
        //player.transform.rotation = Quaternion.Euler(spawnRot);
        //player.transform.SetPositionAndRotation(playerSpawn.position, playerSpawn.rotation);
        Instantiate(levels[numTutorial], environment).transform.SetAsFirstSibling();
        //player.GetComponent<PlayerTrick>().lAlways = true;
        //this.CallDelay(StartEnemy, delaySpawnEnemy);
    }

    public void NextTutorial() //when player completes goal, go to next tutorial
    {
        numTutorial++; //increase index of tutorial
        int numTut = numTutorial; //get index of tutorial
        if (numTut >= tutMessages.Length) //if there are no more tutorials and goals left, end tutorial
        {
            player.GetComponent<PlayerUI>().TextFeedback("Tutorial Finished!", 0);
            this.CallDelay(ResetGame, 3f);
            GameProgress.LevelComplete(0);
            return;
        }

        //Set UI for next goal:
        StartCoroutine(WaitForFirstTutorial(numTut)); //comment this line for faster testing
        gText.text = gMessagesStart[numTut].Replace("/newline/", "\n") + countGoal.ToString() + gMessagesEnd[numTut]; //goal
        //Set UI for next counter:
        cTextHeader.text = cHeaders[numTut]; //action for counter
        //Set UI for next tutorial:
        tutKey.sprite = keys[numTut]; //key input
        tutText.text = tutMessages[numTut]; //action for key

        //toggle UI visibility:
        foreach (Transform gChild in gCanvas)
            gChild.gameObject.SetActive(false);
        foreach (Transform cChild in cCanvas)
            cChild.gameObject.SetActive(false);
        foreach (Transform tutChild in tutCanvas)
            tutChild.gameObject.SetActive(false);
        //reset counter:
        csValue = 0f;
        count = 0;

        gCam.SetActive(true); //toggle on goal camera
        //destroy players, level, and world space UI, if they exist:
        if (player != null)
            Destroy(player);
        if (environment.childCount > envChildCountStart)
            Destroy(environment.GetChild(0).gameObject);
        if (environment.Find("Wall Markers").childCount != 0)
        {
            foreach (Transform wallMarkerChild in environment.Find("Wall Markers"))
                Destroy(wallMarkerChild.gameObject);
        }

        //for faster testing: //Also comment out line 212
        //foreach (Transform gChild in gCanvas) //toggle UI for goal canvas
        //    gChild.gameObject.SetActive(true);
        //gInitialVideoDisplay.SetActive(false); //toggle display of tutorial
        //gSkipPopUp.SetActive(false); //toggle skip text
        //gLoopVideoPlayer.clip = gLoopClips[numTut]; //2nd part of video tutorial plays
        //StartPlayerTutorial();
        //print("Faster Testing Block is Uncommented");
    }
    IEnumerator WaitForFirstTutorial(int numTut)
    {
        gInitialVideoPlayer.clip = gInitialClips[numTut]; //beginning video tutorial plays
        gInitialVideoDisplay.SetActive(true); //toggle display of tutorial
        gSkipPopUp.SetActive(false); //toggle skip text
        yield return new WaitForSeconds((float)gInitialClips[numTut].length);

        gInitialVideoPlayer.clip = gInitialClips[numTut]; //play again
        gSkipPopUp.SetActive(true); //toggle skip text
        float timeOut = 0;

        while (!Input.GetKeyDown(KeyCode.Space) && timeOut < (float)gInitialClips[numTut].length * 2) //play video twice unless player skips
        {
            timeOut += Time.deltaTime;
            yield return null;
        }
        foreach (Transform gChild in gCanvas) //toggle UI for goal canvas
            gChild.gameObject.SetActive(true);
        gInitialVideoDisplay.SetActive(false); //toggle display of tutorial
        gSkipPopUp.SetActive(false); //toggle skip text
        gLoopVideoPlayer.clip = gLoopClips[numTut]; //2nd part of video tutorial plays
    }

    public IEnumerator LastCountWaitForLand() //before tutorial finishes for jumping, make sure player lands
    {
        PlayerTrick pt_ = player.GetComponent<PlayerTrick>();
        while (!pt_.isLanding)
            yield return null;
        yield return new WaitForFixedUpdate();
        if (pt_.AnimCheck(0, "Land1"))
            this.CallDelay(NextTutorial, tutTransDelay);
    }

    public void IncreaseCounter() //when player follows goal, increase count
    {
        if (cCanvas != null)
        {
            count++;
            if (count >= countGoal) //goal completed
            {
                if (numTutorial != 2)
                    this.CallDelay(NextTutorial, tutTransDelay);
                else
                    StartCoroutine(LastCountWaitForLand());
            }
        }
    }
    public void DecreaseCounter() //when player doesn't land after jump
    {
        if (cCanvas != null && numTutorial == 2 && count > 0)
            count--;
    } 
    #endregion

    void StartLevel()
    {
        GameObject level_ = Instantiate(levels[0], environment);
        level_.transform.SetAsFirstSibling();
        level_.AddComponent<LevelPrep>();

        player = Instantiate(playerPref);
        enemy1 = Instantiate(enemyPref);
        enemy1.GetComponent<EnemyTrick>().enemyNum = 1;
        enemy2 = Instantiate(enemyPref);
        enemy2.GetComponent<EnemyTrick>().enemyNum = 2;
        StartCoroutine(Spawn());

    }

    public void LevelFinished(int level_)
    {
        player.GetComponent<PlayerUI>().TextFeedback("Level Finished!", 0);
        this.CallDelay(ResetGame, 3f);
        GameProgress.LevelComplete(level_);
    }

    public bool GetEnemyAction(int enemyNum)
    {
        if (actionsEnemy.Length > actionIndex1) //if there is an action left
        {
            if (enemyNum == 2 && actionsEnemy.Length > actionIndex2) //if actionindex is being given to left enemy
            {
                //make enemy action markers for level:
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
        }
        else
        {
            while (enemy1.GetComponent<EnemyMovement>().gm == null) //wait until this enemy's script can access this script
                yield return null;
            //reset positions and index of all players:
            player.transform.SetPositionAndRotation(playerSpawn.position, playerSpawn.rotation);

            enemy1.GetComponent<EnemyMovement>().ResetPlayer();
            enemy1.transform.SetPositionAndRotation(enemySpawn.position, enemySpawn.rotation);
            actionIndex1 = 0;

            enemy2.GetComponent<EnemyMovement>().ResetPlayer();
            enemy2.transform.SetPositionAndRotation(enemySpawn.GetChild(0).position, enemySpawn.rotation);
            actionIndex2 = 0;

            EnemyTrick et1_ = enemy1.GetComponent<EnemyTrick>();
            EnemyTrick et2_ = enemy2.GetComponent<EnemyTrick>();
            PlayerTrick pt_ = player.GetComponent<PlayerTrick>();
            yield return new WaitForEndOfFrame();
            while (et1_.AnimCheck(0, "Exit") || et2_.AnimCheck(0, "Exit") || pt_.AnimCheck(0, "Exit")) //wait until all players reset
                yield return null;
            //reset speed for all players:
            enemy1.GetComponent<EnemyMovement>().velocityZ = initialSpeedEnemy;
            enemy2.GetComponent<EnemyMovement>().velocityZ = initialSpeedEnemy;
            player.GetComponent<PlayerMovement>().velocityZ = initialSpeedPlayer;

            gameover = false;
            //UsefulShortcuts.ClearConsole();
        }
    }

    private void ResetGame() => SceneManager.LoadScene(0);
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