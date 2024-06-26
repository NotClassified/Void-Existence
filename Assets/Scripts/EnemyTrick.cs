﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class EnemyTrick : MonoBehaviour
{
    public int enemyNum;
    bool waitingToPunchPlayer = false;
    #region COMPONENTS
    private CharacterController cc;
    private Animator anim;
    private GameManager gm;
    [SerializeField]
    private EnemyMovement em;
    [SerializeField]
    MultiAimConstraint aimContraint;
    [SerializeField]
    RigBuilder rigPunch;
    #endregion
    #region ANIMATION BOOLEANS
    public bool defaultMove;
    public bool inAir;
    public bool isJumping;
    public bool isClimbing;
    public bool isLanding = false;
    public bool isPunching;
    bool[] actionPrevent;
    #endregion
    #region RAYCAST
    RaycastHit[] hits;
    Vector3[] raypos;
    [SerializeField]
    float[] distances;
    public LayerMask groundMask;
    public LayerMask wallMask;
    #endregion
    #region FALLING & LANDING //d-distance l-land
    public bool isGrounded = true;
    [SerializeField]
    float ldInputGap;
    [SerializeField]
    float dDominator;
    [SerializeField]
    float lResetDelay;
    [SerializeField]
    Vector3 rayDir;
    #endregion
    #region JUMPING //j-jump
    [SerializeField]
    float jResetDelay;
    [SerializeField]
    float jSpeedResetDelay;
    [SerializeField]
    float jBoost;
    [SerializeField]
    float jDurationBoost;
    [SerializeField]
    float jDecayBoost;
    #endregion
    #region WALL CLIMB //wc-wall climb
    public float wcClipEnd;
    [SerializeField]
    float reposDuration;
    [SerializeField]
    Vector3 inAirClimbOffset;
    #endregion
    #region PUNCH
    private Coroutine enemyWaitForPlayerPunchRoutine;
    private Coroutine enemyPunchRoutine;
    [SerializeField]
    Transform punchEndPosition;
    [SerializeField]
    Vector3 punchOffset;
    [SerializeField]
    float punchDuration;
    [SerializeField]
    float punchAimWeightDuration;
    float punchLayerWeight;
    [SerializeField]
    float punchWeightSpeed;
    #endregion
    #region HASHES
    private int hashFall;
    private int hashJumpDown;
    private int hashWallClimb;
    private int hashAirToClimb;
    private int hashClimbFail;
    private int hashClimbSpeed;
    private int hashLand;
    private int hashPunch;
    #endregion

    public bool startMethodCalled = false;
    void Start()
    {
        anim = GetComponent<Animator>();
        gm = FindObjectOfType<GameManager>();
        cc = GetComponent<CharacterController>();

        actionPrevent = new bool[3];
        hits = new RaycastHit[10];
        raypos = new Vector3[10];

        hashFall = Animator.StringToHash("Fall");
        hashJumpDown = Animator.StringToHash("jumpDown");
        hashWallClimb = Animator.StringToHash("Climb");
        hashAirToClimb = Animator.StringToHash("AirToClimb");
        hashClimbFail = Animator.StringToHash("Climb Fail");
        hashClimbSpeed = Animator.StringToHash("Climb Speed");
        hashLand = Animator.StringToHash("Land");
        hashPunch = Animator.StringToHash("Punch");

        startMethodCalled = true;
    }

    public bool AnimCheck(int layer, string state)
    {
        if (anim != null)
            return anim.GetCurrentAnimatorStateInfo(layer).IsName(state); //check if an animation state is playing on a layer
        return false;
    }
    public void ToggleCC_OFF() => cc.enabled = false; //toggle Character Controller component off (collider)
    public void ToggleCC_ON() => cc.enabled = true; //toggle Character Controller component on (collider)

    IEnumerator WCRepos(Transform pos_)
    {
        Vector3 initial = transform.position; //starting position
        //RETARGET REPOSITION DEPENDING ON WHICH SIDE THIS ENEMY IS ON:
        Vector3 pos = new Vector3();
        if (enemyNum == 1)
        {
            pos = pos_.position + new Vector3(-2, 0, 0);
        }
        else
            pos = pos_.position + new Vector3(2, 0, 0);
        float time = 0;
        while (time < reposDuration) //repositioning gradually (ends before player hits wall)
        {
            time += Time.deltaTime;
            transform.position = Vector3.Lerp(initial, pos, time / reposDuration);
            yield return null;
        }
        transform.position = pos; //ensure reposition complete
    }

    void Update()
    {
        if (!em.startMethodCalled || gm.player == null)
            return;

        #region ANIMATION VARS
        isJumping = !AnimCheck(1, "Empty"); //check jumping anims
        isClimbing = AnimCheck(0, "Wall Climb") || AnimCheck(0, "WC Fail") || AnimCheck(0, "Wall Climb In Air"); //check climbing anims
        isLanding = AnimCheck(0, "Land1") || AnimCheck(0, "Land2"); //check if landing
        isPunching = AnimCheck(2, "Punch"); //check if punching player
        defaultMove = !(isJumping || isClimbing || isLanding);

        if (AnimCheck(0, "Exit"))
            anim.SetBool("Exit", true);
        else if (anim.GetBool("Exit"))
            anim.SetBool("Exit", false);
        #endregion

        #region RAYCAST POSITIONS
        raypos[1] = transform.position + Vector3.up * distances[0] + -transform.right * distances[3]; //edge for jumping
        raypos[3] = transform.position + Vector3.up * distances[0] + -transform.right * distances[10]; //edge for punching
        raypos[2] = transform.position + Vector3.up * distances[6]; //walls
        //Debug.DrawLine(raypos[2], raypos[2] + Vector3.back * distances[7], Color.cyan);
        //Debug.DrawLine(raypos[2] + Vector3.up * .1f, raypos[2] + Vector3.back * distances[8] + Vector3.up * .1f, Color.red);
        //Vector3 visualOffset = new Vector3(0, 0, -.1f);
        //Debug.DrawLine(raypos[0] + visualOffset, raypos[0] + Vector3.down * distances[5] + visualOffset, Color.cyan);
        //Debug.DrawLine(raypos[0], raypos[0] + Vector3.down * distances[4], Color.red);
        //Debug.DrawRay(raypos[0], rayDir, Color.cyan, .1f); 
        #endregion

        #region LANDING, FALLING, & GROUNDCHECK
        distances[4] = (-em.fallvelocity.y) / dDominator; //distance for landing
        distances[5] = distances[4] + ldInputGap; //distance for landing input by player

        raypos[0] = transform.position + Vector3.up * distances[0] + -transform.right * distances[2]; //raycast position for checking if player is grounded
        //check if player is in air (not grounded)
        if (!isLanding && !isClimbing && cc.enabled && !Physics.Raycast(raypos[0], Vector3.down, out hits[3], distances[0], groundMask)) 
        {
            isGrounded = false;

            if (Physics.Raycast(raypos[0], rayDir, out hits[0], distances[4], groundMask)) //start landing animation
            {
                //Debug.Log(pm.fallvelocity.y);
                //Debug.Log(distances[4]);
                //check if close enough to platform
                if (Physics.Raycast(raypos[0], Vector3.down, out hits[0], distances[5], groundMask))
                {
                    anim.SetBool(hashLand, true);
                    anim.SetBool(hashFall, !gm.GetEnemyAction());
                }
                else //not close enough to platform
                {
                    cc.Move(new Vector3(0, 0, .1f)); //move player back to avoid platform
                    em.velocityZ = 0; //stop player from going forward
                }
            }
            //air to wall climb
            else if (Physics.Raycast(raypos[2] + inAirClimbOffset, Vector3.back, out hits[2], distances[7], wallMask))
            {
                anim.SetFloat(hashClimbSpeed, 1); //set speed of wall climb based on forward velocity

                if (enemyNum == 1)//reposition depending on which side the enemy is on
                {
                    transform.position = hits[2].transform.GetChild(0).position + new Vector3(-2, 0, 0);
                }
                else
                    transform.position = hits[2].transform.GetChild(0).position + new Vector3(2, 0, 0);
                wcClipEnd = Time.time + 2.07999f; //find when animation clip will end

                ToggleCC_OFF(); //disable collider
                em.velocityZ = 9; //set forward speed of player

                anim.SetBool(hashAirToClimb, true);
                anim.SetBool(hashClimbFail, false);
                anim.SetBool(hashWallClimb, true);
            }
        }
        else //isgrounded
        {
            isGrounded = true; //is also true when landing
            anim.SetBool(hashLand, false); //prevent loop of landing

            if (hits[3].transform != null)
            {
                if (hits[3].transform.CompareTag("Level2"))
                    gm.SetEnemyLevel(2);
                else
                    gm.SetEnemyLevel(1);
            }
        }
        anim.SetBool("IsGrounded", isGrounded); //correlate animation vars 
        #endregion

        #region WALL CLIMBING & JUMPING
        //check if enemy is in front of a wall
        if (defaultMove && !isPunching && Physics.Raycast(raypos[2], Vector3.back, out hits[2], distances[7], wallMask)) 
        {
            if (!actionPrevent[1] && gm.GetEnemyAction()) //check if enemy should wall climb
            {
                anim.SetBool(hashClimbFail, false); //player succeeded wall climb

                float wallClimbSpeed = (em.velocityZ - 6) / 10 + 1;
                if (wallClimbSpeed < 1)
                    wallClimbSpeed = 1;
                anim.SetFloat(hashClimbSpeed, wallClimbSpeed); //set speed of wall climb based on forward velocity

                string num = hits[2].transform.name.Substring(4); //get the correct wall number
                StartCoroutine(WCRepos(hits[2].transform.GetChild(0))); //gradually reposition player for wall climb animation
                wcClipEnd = Time.time + 2.666667f / wallClimbSpeed; //find when animation clip will end

                this.CallDelay(ToggleCC_OFF, 1f); //disable collider
                em.velocityZ = 9; //set forward speed of player
                anim.SetBool(hashWallClimb, true);
            }
            else
                actionPrevent[1] = true; //prevent wall climb
        }
        else
            actionPrevent[1] = false; //after wall climb or until enemy can't wall climb (default value)

        //check if enemy is too close to wall
        if (defaultMove && !isPunching && Physics.Raycast(raypos[2], Vector3.back, out hits[2], distances[8], wallMask)) 
        {
            anim.SetBool(hashClimbFail, true); //player failed wall climb
            float wallClimbSpeed = (em.velocityZ - 6) / 10 + 1;
            anim.SetFloat(hashClimbSpeed, wallClimbSpeed); //set speed of wall climb based on forward velocity

            string num = hits[2].transform.name.Substring(4); //get the correct wall number
            //REPOSITION PLAYER FOR WALL CLIMB ANIMATION DEPENDING ON WHICH SIDE THIS ENEMY IS ON:
            if (enemyNum == 1)
                transform.position = hits[2].transform.GetChild(0).position + new Vector3(-2, 0, 0);
            else
                transform.position = hits[2].transform.GetChild(0).position + new Vector3(2, 0, 0);

            wcClipEnd = Time.time + 4f / wallClimbSpeed; //find when animation clip will end
            this.CallDelay(ToggleCC_OFF, 1f); //disable collider
            anim.SetBool(hashWallClimb, true);
        }

        //check if enemy is by an edge to jump off of and not in front of a wall (by raycasts respectively)
        if (defaultMove && !isPunching && !Physics.Raycast(raypos[1], Vector3.down, out hits[1], distances[1], groundMask) &&
            !Physics.Raycast(raypos[2], Vector3.back, out hits[2], distances[7], wallMask)) 
        {
            if (!actionPrevent[2] && gm.GetEnemyAction()) //check if enemy should jump
            {
                em.velocityZ = 12; //boost player forward
                em.StartCoroutine(em.BoostPlayer(jBoost, jDurationBoost, jDecayBoost)); //boost player forward more
                anim.SetBool(hashJumpDown, true);
            }
            actionPrevent[2] = true; //prevent jump
        }
        else
            actionPrevent[2] = false; //after jump or until enemy can't jump (default value)
        #endregion

        #region PUNCHING
        //checking if enemy caught up to player
        if (!isPunching && transform.position.z + 2 < gm.player.transform.position.z && !waitingToPunchPlayer
             && !gm.eaStopPlayerForActionMarkers) 
        {
            waitingToPunchPlayer = true;
            gm.StartGameOverRoutine();
            enemyWaitForPlayerPunchRoutine = StartCoroutine(WaitForPlayerPunch(gm.player));
        }
        if (isPunching)
        {
            punchLayerWeight += Time.deltaTime * punchWeightSpeed; //transition to layer 2 (punch animation)

            if (!(punchLayerWeight > 1)) //check if out of bounds
                anim.SetLayerWeight(2, punchLayerWeight); //correlate vars
            else //if over 1, then equal 1
                anim.SetLayerWeight(2, 1); //correlate vars
        }
        //if(enemyNum == 2)
        //{
        //    Debug.DrawLine(raypos[3], raypos[3] + Vector3.down * distances[1], Color.red); //wall check raycast
        //    Debug.DrawLine(raypos[2], raypos[2] + Vector3.back * distances[7], Color.red); //edge check raycast
        //}

        #endregion
    }

    #region PUNCHING METHODS
    IEnumerator WaitForPlayerPunch(GameObject player_)
    {
        if (enemyNum == 1 || enemyNum == 2) //wait to stop if this enemy is NOT extra enemy
        {
            //wait until enemy isn't by edge and isn't close to wall (with Raycasts respectively) and in default animation state and not above player
            while (!defaultMove || !Physics.Raycast(raypos[3], Vector3.down, out hits[1], distances[1], groundMask) ||
                Physics.Raycast(raypos[2], Vector3.back, out hits[2], distances[7], wallMask) || !gm.IsPlayerAndEnemyOnSameLevel())
            {
                //if enemy falls behind player before stopping
                if (transform.position.z > player_.transform.position.z)
                {
                    gm.StopGameOverRoutine();
                    StopCoroutine(enemyWaitForPlayerPunchRoutine);
                    waitingToPunchPlayer = false;
                }
                yield return null;
            }
        }
        //print("ready to punch");
        //if enemy is ahead of player or if player isn't in default animation state
        //if (transform.position.z + 2 < player_.transform.position.z)
        //{
            EnemyStopRunning(); //stop enemy to wait for player
            rigPunch.enabled = true; //for the aim constraint

            //yield until player passes enemy while in a default animation
            while (transform.position.z < player_.transform.position.z  || !player_.GetComponent<PlayerTrick>().defaultMove)
                yield return null;
            enemyPunchRoutine = StartCoroutine(EnemyPunch());
            StartCoroutine(player_.GetComponent<PlayerTrick>().PunchedByEnemy());
        //}
        //else //punch player
        //{
        //    enemyPunchRoutine = StartCoroutine(EnemyPunch());
        //    StartCoroutine(player_.GetComponent<PlayerTrick>().PunchedByEnemy());
        //}

    }
    public void EnemyStopRunning() => anim.SetBool("Stop", true);

    public IEnumerator EnemyPunch()
    {
        //CHANGE OFFSET IF ENEMY IS ON THE RIGHTSIDE & FLIP PUNCH ANIMATION IF ENEMY IS ON THE LEFTSIDE
        Vector3 punchOffset_;
        if (enemyNum == 1 || enemyNum == 3)
            punchOffset_ = new Vector3(-punchOffset.x, 0, punchOffset.z);
        else
        {
            punchOffset_ = punchOffset;
            anim.SetFloat("FlipForPunch", 0);
        }

        //IF EXTRA ENEMY IS PUNHCING, TELL PLAYERTRICK SCRIPT TO FLIP ANIMATION ACCORDINGLY
        PlayerTrick _pt = gm.player.GetComponent<PlayerTrick>();
        if (enemyNum == 3)
        {
            _pt.extraEnemyIsPunching = true;
            _pt.extraEnemyNumber = 3;
        }
        else if (enemyNum == 4)
        {
            _pt.dodgedEnemy = false;
            _pt.extraEnemyIsPunching = true;
            _pt.extraEnemyNumber = 4;
        }

        anim.SetBool(hashPunch, true); //punch
        yield return new WaitForEndOfFrame();
        anim.SetBool(hashPunch, false); //prevent loop of punch animation

        //TRANSITION ENEMY ROTATION TO FACE THE PLAYER:
        float time = 0f;
        Transform posPlayer = gm.player.transform;
        while (time < punchAimWeightDuration)
        {
            time += Time.deltaTime;
            punchEndPosition.position = posPlayer.position;
            aimContraint.weight = Mathf.Lerp(0, 1, time / punchAimWeightDuration);
            yield return null;
        }
        aimContraint.weight = 1;

        //MOVE ENEMY TOWARDS PLAYER
        Vector3 initialPos = transform.position;
        time = 0;
        while (time < punchDuration)
        {
            time += Time.deltaTime;
            punchEndPosition.position = posPlayer.position;
            transform.position = Vector3.Lerp(initialPos, punchEndPosition.position + punchOffset_, time / punchDuration);
            yield return null;
        }
        transform.position = punchEndPosition.position + punchOffset_;

        /* ENEMY HAS PUNCHED PLAYER */
        gm.playerPunchedByEnemy = true;

        //Time.timeScale = 0.05f; //for seeing if offset is correct

        //TRANSITION ENEMY ROTATION BACK TO FACING FORWARD
        time = 0;
        while (time < punchAimWeightDuration)
        {
            time += Time.deltaTime;
            aimContraint.weight = Mathf.Lerp(1, 0, time / punchAimWeightDuration);
            yield return null;
        }
        aimContraint.weight = 0;

        rigPunch.enabled = false; //prevent the aim constraint from affecting other aimations
    }

    public void StartEnemyPunchRoutine() => enemyPunchRoutine = StartCoroutine(EnemyPunch());

    public void StopEnemyPunchRoutine()
    {
        //STOP COROUTINES
        if (enemyPunchRoutine != null)
            StopCoroutine(enemyPunchRoutine);
        if (enemyWaitForPlayerPunchRoutine != null)
            StopCoroutine(enemyWaitForPlayerPunchRoutine);

        //RESETTING VARS:
        aimContraint.weight = 0;
        rigPunch.enabled = false;
        punchLayerWeight = 0;
        anim.SetLayerWeight(2, punchLayerWeight);
        anim.SetBool("Stop", false);
        anim.SetBool(hashPunch, false);

    }

    #endregion

}
