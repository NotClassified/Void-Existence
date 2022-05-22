﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTrick : MonoBehaviour
{
    #region COMPONENTS
    private CharacterController cc;
    private Animator anim;
    private GameManager gm;
    [SerializeField]
    private PlayerUI pUI;
    [SerializeField]
    private PlayerMovement pm;
    #endregion
    #region ANIMATION BOOLEANS
    public bool defaultMove;
    public bool inAir;
    public bool isJumping;
    public bool isClimbing;
    public bool isLanding = false;
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
    public bool attemptedLand = false;
    public bool isGrounded = true;
    public bool lAlways = false;
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
    public bool jAlways = false;
    bool autoJumped = false;
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
    bool attemptedJump = false;
    #endregion
    #region WALL CLIMB //wc-wall climb
    public float wcClipEnd;
    public bool attemptedClimb = false;
    public bool wcAlways;
    bool autoWallClimb = false;
    [SerializeField]
    float reposDuration;
    #endregion
    #region HASHES
    private int hashFall;
    private int hashWallClimb;
    private int hashClimbFail;
    private int hashClimbSpeed;
    private int hashLand;
    private int hashPunched;
    #endregion

    public bool startMethodCalled = false;
    void Start()
    {
        anim = GetComponent<Animator>();
        gm = FindObjectOfType<GameManager>();
        cc = GetComponent<CharacterController>();
        hits = new RaycastHit[10];
        raypos = new Vector3[10];
        hashFall = Animator.StringToHash("Fall");
        hashWallClimb = Animator.StringToHash("Climb");
        hashClimbFail = Animator.StringToHash("Climb Fail");
        hashClimbSpeed = Animator.StringToHash("Climb Speed");
        hashLand = Animator.StringToHash("Land");
        hashPunched = Animator.StringToHash("Punched");

        startMethodCalled = true;
    }

    public bool AnimCheck(int layer, string state)
    {
        if(anim != null)
            return anim.GetCurrentAnimatorStateInfo(layer).IsName(state); //check if an animation state is playing on a layer
        return false;
    }
    public void ToggleCC_OFF() => cc.enabled = false; //toggle Character Controller component off (collider)
    public void ToggleCC_ON() => cc.enabled = true; //toggle Character Controller component on (collider)

    public bool JumpDownCheck()
    {
        if (gm.numTutorial == 0) //check if playing landing tutorial
            return false; //don't jump during landing tutorial

        if (!attemptedJump && (defaultMove || (isLanding && anim.GetCurrentAnimatorStateInfo(0).normalizedTime > .8f)) && pm.velocityZ > 5.9f &&
            !Physics.Raycast(raypos[1], Vector3.down, out hits[1], distances[1], groundMask) &&
            !Physics.Raycast(raypos[2], Vector3.back, out hits[2], distances[7], wallMask)) //check if player is by an edge to jump off of and not in front of a wall (by raycasts respectively)
        {
            pm.StartCoroutine(pm.BoostPlayer(jBoost, jDurationBoost, jDecayBoost)); //boost player forward more
            if (gm.numTutorial == 2) //if player is in tutorial for jumping, increase counter
                gm.IncreaseCounter();
            return true;
        }
        else if(defaultMove && !Physics.Raycast(raypos[2], Vector3.back, out hits[2], distances[9], wallMask)) //check if player is not in front of a wall
        {
            attemptedJump = true; //prevent repressing key
            if (pm.velocityZ < 6f) //check if player is below speed limit for jumping
            {
                pUI.TextFeedback("Not Enough Speed To Jump", 4);
                this.CallDelay(ClearJumpingFeedback, jSpeedResetDelay); //if too early then give second chance for jumping
            }
            else //player is too far from edge
            {
                pUI.TextFeedback("Too Early To Jump", 4);
                this.CallDelay(ClearJumpingFeedback, jResetDelay); //if too early then give second chance for jumping
            }
        }
        return false;
    }

    #region WALL CLIMBING METHODS
    public bool WallClimbCheck()
    {
        if (defaultMove && !attemptedClimb && Physics.Raycast(raypos[2], Vector3.back, out hits[2], distances[7], wallMask)) //check if player is in front of a wall and hasn't tried to climb yet
        {
            anim.SetBool(hashClimbFail, false); //player succeeded wall climb
            float wallClimbSpeed = (pm.velocityZ - 6) / 10 + 1;
            anim.SetFloat(hashClimbSpeed, wallClimbSpeed); //set speed of wall climb based on forward velocity
            string num = hits[2].transform.name.Substring(4); //get the correct wall number
            StartCoroutine(WCRepos(hits[2].transform.GetChild(0))); //gradually reposition player for wall climb animation
            wcClipEnd = Time.time + 2.666667f / wallClimbSpeed; //find when animation clip will end
            this.CallDelay(ToggleCC_ON, 1f); //disable collider
            pm.velocityZ = 9; //set forward speed of player
            pUI.TextFeedback("Perfect Climb!", 3);
            if (gm.numTutorial == 1) //if player is in tutorial for wall climbing, increase counter
                gm.IncreaseCounter();
            return true; //play animation
        }
        else if (defaultMove && Physics.Raycast(raypos[2], Vector3.back, out hits[2], distances[9], wallMask)) //check if player is too far from wall
        {
            pUI.TextFeedback("Too Early To Climb", 4);
            attemptedClimb = true; //prevent repressing key
        }
        return false; //don't play animation
    }

    IEnumerator WCRepos(Transform pos)
    {
        Vector3 initial = transform.position; //starting position
        float time = 0;
        while (time < reposDuration) //repositioning gradually (ends before player hits wall)
        {
            time += Time.deltaTime;
            transform.position = Vector3.Lerp(initial, pos.position, time / reposDuration);
            yield return null;
        }
        transform.position = pos.position; //ensure reposition complete
    }
    #endregion

    #region TEXT FEEDBACK
    void ClearTextFeedback() => pUI.TextFeedback("", -1); //empty feedback text
    void ClearLandingTextFeedback()
    {
        if (!isLanding)
        {
            pUI.TextFeedback("", -1); //empty feedback text
            attemptedLand = false; //let player attempt landing again if still in air
        }
        else
            gm.DecreaseCounter();
    }
    void ClearJumpingFeedback()
    {
        if (pUI.GetFeedbackText().Equals("Too Early To Jump") || pUI.GetFeedbackText().Equals("Not Enough Speed To Jump"))
        {
            pUI.TextFeedback("", -1); //empty feedback text
            attemptedJump = false; //let player attempt jumping again
        }
    } 
    #endregion

    void Update()
    {
        if (!pm.startMethodCalled || !pUI.startMethodCalled)
            return;


        isJumping = !AnimCheck(1, "Empty"); //check jumping anims
        isClimbing = AnimCheck(0, "Wall Climb") || AnimCheck(0, "WC Fail"); //check climbing anims
        isLanding = AnimCheck(0, "Land1") || AnimCheck(0, "Land2"); //check if landing
        defaultMove = !(isJumping || isClimbing || isLanding || AnimCheck(0, "Exit")); //false if player is any of these states


        raypos[1] = transform.position + Vector3.up * distances[0] + -transform.right * distances[3];
        raypos[2] = transform.position + Vector3.up * distances[6];
        //Debug.DrawLine(raypos[2], raypos[2] + Vector3.back * distances[7], Color.cyan);
        //Debug.DrawLine(raypos[2] + Vector3.up * .1f, raypos[2] + Vector3.back * distances[8] + Vector3.up * .1f, Color.red);
        Vector3 visualOffset = new Vector3(0, 0, -.1f);
        Debug.DrawLine(raypos[0] + visualOffset, raypos[0] + Vector3.down * distances[5] + visualOffset, Color.cyan);
        Debug.DrawLine(raypos[0], raypos[0] + Vector3.down * distances[4], Color.red);
        Debug.DrawRay(raypos[0], rayDir, Color.cyan, .1f);

        #region LANDING, FALLING, AND GROUNDCHECK

        distances[4] = (-pm.fallvelocity.y) / dDominator; //distance for landing
        distances[5] = distances[4] + ldInputGap; //distance for landing input by player

        raypos[0] = transform.position + Vector3.up * distances[0] + -transform.right * distances[2]; //raycast position for checking if player is grounded
        if (!isLanding && !isClimbing && cc.enabled && !Physics.Raycast(raypos[0], Vector3.down, out hits[0], distances[0], groundMask)) //check if player is in air (not grounded)
        {
            isGrounded = false;

            if (Input.GetKeyDown(KeyCode.S) && !attemptedLand) //landing input
            {
                if (Physics.Raycast(raypos[0], rayDir, out hits[0], distances[5], groundMask)) //check if ground is within distance to land
                {
                    anim.SetBool(hashFall, false); //land success
                    if (gm.numTutorial == 0) //if player is in tutorial for landing, increase counter
                        gm.IncreaseCounter();
                    pUI.TextFeedback("Perfect Landing!", 3);
                }
                else
                {
                    anim.SetBool(hashFall, !lAlways); //land fail unless set to always land
                    pUI.TextFeedback("Early Landing", 4);
                    this.CallDelay(ClearLandingTextFeedback, lResetDelay); //if too early then give second chance for landing
                }
                attemptedLand = true; //prevent repressing key
            }
            if (Physics.Raycast(raypos[0], rayDir, out hits[0], distances[4], groundMask)) //start landing animation
            {
                //Debug.Log(pm.fallvelocity.y);
                //Debug.Log(distances[4]);
                if (Physics.Raycast(raypos[0], Vector3.down, out hits[0], distances[5], groundMask)) //check if close enough to platform
                {
                    if (!attemptedLand) //check if player didn't do anything or pressed key too late
                    {
                        anim.SetBool(hashFall, !lAlways); //land fail unless set to always land
                        pUI.TextFeedback("Late Landing", 4);
                        gm.DecreaseCounter(); //if player is in tutorial for jumping, decrease counter
                    }
                    anim.SetBool(hashLand, true);
                    if (anim.GetBool(hashFall))
                        this.CallDelay(ClearTextFeedback, 1.1f);
                    else
                        this.CallDelay(ClearTextFeedback, .6f);
                }
                else //not close enough to platform
                {
                    cc.Move(new Vector3(0, 0, .1f)); //move player back to avoid platform
                    pm.velocityZ = 0; //stop player from going forward
                    pUI.TextFeedback("", -1); //empty feedback text
                }
            }
        }
        else //isgrounded
        {
            isGrounded = true; //is also true when landing
            anim.SetBool(hashLand, false); //prevent loop of landing
            attemptedLand = false;
            //anim.SetBool(hashFall, true);
            //pUI.tShowed[0] = false; //keep playing tutorial
        }
        anim.SetBool("IsGrounded", isGrounded); //correlate animation vars 
        #endregion

        #region WALL CLIMB FAIL & AUTO JUMP
        if (defaultMove && !pUI.GetFeedbackText().Equals("Perfect Climb!") &&
            Physics.Raycast(raypos[2], Vector3.back, out hits[2], distances[8], wallMask)) //check if player is too close to wall
        {
            if (wcAlways && !autoWallClimb)
            {
                anim.SetBool(hashWallClimb, WallClimbCheck());
                autoWallClimb = true;
            }
            else
            {
                anim.SetBool(hashClimbFail, true); //player failed wall climb
                float wallClimbSpeed = 1.3f;
                anim.SetFloat(hashClimbSpeed, wallClimbSpeed); //set speed of wall climb based on forward velocity
                string num = hits[2].transform.name.Substring(4); //get the correct wall number
                transform.position = hits[2].transform.GetChild(0).position; //reposition player for wall climb animation
                wcClipEnd = Time.time + 4f / wallClimbSpeed; //find when animation clip will end
                this.CallDelay(ToggleCC_OFF, 1f); //disable collider
                anim.SetBool(hashWallClimb, true);
                if (pm.velocityZ < 6f) //check if player is below speed limit for climbing
                    pUI.TextFeedback("Not Enough Speed To Climb", 4);
                else if (!attemptedClimb) //check if player didn't do anything or pressed key too late
                {
                    pUI.TextFeedback("Too Late To Climb", 4);
                }
            }
        }

        if (jAlways && !autoJumped && defaultMove && pm.velocityZ > 5.9f &&
            !Physics.Raycast(raypos[1], Vector3.down, out hits[1], distances[1], groundMask) &&
            !Physics.Raycast(raypos[2], Vector3.back, out hits[2], distances[7], wallMask))
        {
            autoJumped = true;
            this.CallDelay(DelayForAutoJump, .2f);
        }
        if (autoJumped && isLanding)
            autoJumped = false;

        //if (defaultMove && Physics.Raycast(raypos[2], Vector3.back, out hits[2], distances[10], wallMask))
        //{
        //    anim.SetBool(hashWallClimb, WallClimbCheck());
        //} 
        #endregion

    }

    public IEnumerator PunchedByEnemy()
    {
        ToggleCC_OFF();
        yield return new WaitForSeconds(.36f);
        anim.SetBool(hashPunched, true);
        EnemyTrick et_ = gm.enemy2.GetComponent<EnemyTrick>();
        while (et_.isPunching == true)
            yield return null;
        anim.SetBool(hashPunched, false);

    }

    void DelayForAutoJump() => anim.SetBool("jumpDown", JumpDownCheck());
}
