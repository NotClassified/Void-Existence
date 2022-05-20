﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTrick : MonoBehaviour
{
    #region COMPONENTS
    private CharacterController cc;
    private Animator anim;
    private GameManager gm;
    [SerializeField]
    private EnemyMovement em;
    public int enemyNum;
    #endregion
    #region ANIMATION BOOLEANS
    public bool defaultMove;
    public bool inAir;
    public bool isJumping;
    public bool isClimbing;
    public bool isLanding = false;
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
    #endregion
    #region HASHES
    private int hashFall;
    private int hashJumpDown;
    private int hashWallClimb;
    private int hashClimbFail;
    private int hashClimbSpeed;
    private int hashLand;
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
        hashClimbFail = Animator.StringToHash("Climb Fail");
        hashClimbSpeed = Animator.StringToHash("Climb Speed");
        hashLand = Animator.StringToHash("Land");

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
        if (!em.startMethodCalled)
            return;


        isJumping = !AnimCheck(1, "Empty"); //check jumping anims
        isClimbing = AnimCheck(0, "Wall Climb") || AnimCheck(0, "WC Fail"); //check climbing anims
        isLanding = AnimCheck(0, "Land1") || AnimCheck(0, "Land2"); //check if landing
        defaultMove = !(isJumping || isClimbing || isLanding);


        raypos[1] = transform.position + Vector3.up * distances[0] + -transform.right * distances[3];
        raypos[2] = transform.position + Vector3.up * distances[6];
        //Debug.DrawLine(raypos[2], raypos[2] + Vector3.back * distances[7], Color.cyan);
        //Debug.DrawLine(raypos[2] + Vector3.up * .1f, raypos[2] + Vector3.back * distances[8] + Vector3.up * .1f, Color.red);
        //Vector3 visualOffset = new Vector3(0, 0, -.1f);
        //Debug.DrawLine(raypos[0] + visualOffset, raypos[0] + Vector3.down * distances[5] + visualOffset, Color.cyan);
        //Debug.DrawLine(raypos[0], raypos[0] + Vector3.down * distances[4], Color.red);
        //Debug.DrawRay(raypos[0], rayDir, Color.cyan, .1f);

        #region LANDING AND GROUND CHECK
        distances[4] = (-em.fallvelocity.y) / dDominator; //distance for landing
        distances[5] = distances[4] + ldInputGap; //distance for landing input by player

        raypos[0] = transform.position + Vector3.up * distances[0] + -transform.right * distances[2]; //raycast position for checking if player is grounded
        if (!isLanding && !isClimbing && cc.enabled && !Physics.Raycast(raypos[0], Vector3.down, out hits[0], distances[0], groundMask)) //check if player is in air (not grounded)
        {
            isGrounded = false;

            if (Physics.Raycast(raypos[0], rayDir, out hits[0], distances[4], groundMask)) //start landing animation
            {
                //Debug.Log(pm.fallvelocity.y);
                //Debug.Log(distances[4]);
                if (Physics.Raycast(raypos[0], Vector3.down, out hits[0], distances[5], groundMask)) //check if close enough to platform
                {
                    anim.SetBool(hashFall, !gm.GetEnemyAction(enemyNum));
                    anim.SetBool(hashLand, true);
                }
                else //not close enough to platform
                {
                    cc.Move(new Vector3(0, 0, .1f)); //move player back to avoid platform
                    em.velocityZ = 0; //stop player from going forward
                }
            }
        }
        else //isgrounded
        {
            isGrounded = true; //is also true when landing
            anim.SetBool(hashLand, false); //prevent loop of landing
        }
        anim.SetBool("IsGrounded", isGrounded); //correlate animation vars 
        #endregion

        if (defaultMove && Physics.Raycast(raypos[2], Vector3.back, out hits[2], distances[7], wallMask)) //check if enemy is in front of a wall
        {
            if(!actionPrevent[1] && gm.GetEnemyAction(enemyNum))
            {
                anim.SetBool(hashClimbFail, false); //player succeeded wall climb
                float wallClimbSpeed = (em.velocityZ - 6) / 10 + 1;
                anim.SetFloat(hashClimbSpeed, wallClimbSpeed); //set speed of wall climb based on forward velocity
                string num = hits[2].transform.name.Substring(4); //get the correct wall number
                StartCoroutine(WCRepos(hits[2].transform.GetChild(0))); //gradually reposition player for wall climb animation
                wcClipEnd = Time.time + 2.666667f / wallClimbSpeed; //find when animation clip will end
                this.CallDelay(ToggleCC_OFF, 1f); //disable collider
                em.velocityZ = 9; //set forward speed of player
                anim.SetBool(hashWallClimb, true);
            }
            else
                actionPrevent[1] = true;
        }
        else
            actionPrevent[1] = false;

        if (defaultMove && !Physics.Raycast(raypos[1], Vector3.down, out hits[1], distances[1], groundMask) &&
            !Physics.Raycast(raypos[2], Vector3.back, out hits[2], distances[7], wallMask)) //check if player is by an edge to jump off of and not in front of a wall (by raycasts respectively)
        {
            if (!actionPrevent[2] && gm.GetEnemyAction(enemyNum))
            {
                em.velocityZ = 12; //boost player forward
                em.StartCoroutine(em.BoostPlayer(jBoost, jDurationBoost, jDecayBoost)); //boost player forward more
                anim.SetBool(hashJumpDown, true);
            }
            actionPrevent[2] = true;
        }
        else
            actionPrevent[2] = false;

        if ((defaultMove && Physics.Raycast(raypos[2], Vector3.back, out hits[2], distances[8], wallMask))) //check if player is too close to wall
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
    }
}
