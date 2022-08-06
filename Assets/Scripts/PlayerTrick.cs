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
    public bool isClimbingFail;
    public bool isLanding = false;
    public bool isPunched;
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
    public bool landAlways = false;
    bool firstAction = true;
    bool doneFirstAction = false;
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
    Coroutine wallClimbFailRoutine;
    public float wcClipEnd;
    public bool attemptedClimb = false;
    public bool wcAlways;
    [SerializeField]
    float reposDuration;
    #endregion
    #region HASHES
    private int hashFall;
    private int hashWallClimb;
    private int hashClimbFail;
    private int hashClimbSpeed;
    private int hashLand;
    private int hashJumpDown;
    private int hashPunched;
    #endregion
    #region PUNCHED & DODGE
    public bool dodgeAlways = false;
    [SerializeField]
    float punchedWeightSpeed;
    float punchedLayerWeight;
    bool attemptDodge = false;
    bool dodgeNow = false;
    [SerializeField]
    float dodgeDelay;
    public bool dodgedEnemy = false;
    [SerializeField]
    float dodgeWeightSpeed;
    [SerializeField]
    float unDodgeWeightSpeed;
    Coroutine dodgeMashPreventRoutine;
    Coroutine dodgeEnemyRoutine;
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
        hashJumpDown = Animator.StringToHash("jumpDown");
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
        //check if player is by an edge to jump off of and not in front of a wall (by raycasts respectively) and hasn't finished level
        if (!attemptedJump && (defaultMove || (isLanding && anim.GetCurrentAnimatorStateInfo(0).normalizedTime > .8f)) && pm.velocityZ > 5.9f &&
            !Physics.Raycast(raypos[1], Vector3.down, out hits[1], distances[1], groundMask) &&
            !Physics.Raycast(raypos[2], Vector3.back, out hits[2], distances[7], wallMask)) 
        {
            pm.StartCoroutine(pm.BoostPlayer(jBoost, jDurationBoost, jDecayBoost)); //boost player forward more
            if (gm.tutNumber == 3) //if player is in tutorial for jumping, increase counter
                gm.IncreaseCounter();
            anim.SetBool(hashJumpDown, true);
            return true;
        }
        //check if player is not in front of a wall
        else if (defaultMove && !Physics.Raycast(raypos[2], Vector3.back, out hits[2], distances[9], wallMask)) 
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
        else if (isLanding)
        {
            pUI.TextFeedback("Too Early To Jump", 4);
            //this.CallDelay(ClearJumpingFeedback, jResetDelay); //if too early then give second chance for jumping
        }
        return false;
    }

    #region WALL CLIMBING METHODS
    public bool WallClimbCheck()
    {
        //check if player hasn't tried to climb yet and is in front of a wall
        if (defaultMove && !attemptedClimb && !pUI.GetFeedbackText().Equals("Too Late To Climb") && 
            Physics.Raycast(raypos[2], Vector3.back, out hits[2], distances[7], wallMask))
        {
            if (gm.tutNumber == 2 && !doneFirstAction) //if in tutorial and frozen, unfreeze, otherwise prevent wallclimb
            {
                if (Time.timeScale == 0) //if frozen, unfreeze
                {
                    doneFirstAction = true;
                    Time.timeScale = gm.timeScale / 100;
                    gm.LightUpInputText(false); //unlight the input text for wall climb
                }
                else
                    return false;
            }

            //PLAYER SUCCEEDED WALL CLIMB, PLAY ANIMATION:
            pUI.TextFeedback("Perfect Climb!", 3);
            anim.SetBool(hashClimbFail, false);
            anim.SetBool(hashWallClimb, true);

            float wallClimbSpeed = (pm.velocityZ - 6) / 10 + 1;
            if (wallClimbSpeed < 1)
                wallClimbSpeed = 1;
            anim.SetFloat(hashClimbSpeed, wallClimbSpeed); //set speed of wall climb based on forward velocity

            string num = hits[2].transform.name.Substring(4); //get the correct wall number
            StartCoroutine(WCRepos(hits[2].transform.GetChild(0))); //gradually reposition player for wall climb animation
            wcClipEnd = Time.time + 2.666667f / wallClimbSpeed; //find when animation clip will end

            this.CallDelay(ToggleCC_ON, 1f); //disable collider
            pm.velocityZ = 9; //set forward speed of player

            if (gm.tutNumber == 2) //if player is in tutorial for wall climbing, increase counter
                gm.IncreaseCounter();
            return true; //play animation
        }
        //check if player is too far from wall
        else if (defaultMove && !attemptedClimb && Physics.Raycast(raypos[2], Vector3.back, out hits[2], distances[9], wallMask)) 
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
    void ClearTextFeedback()
    {
        if (pUI.GetFeedbackText().Equals("Early Landing") || pUI.GetFeedbackText().Equals("Late Landing")
            || pUI.GetFeedbackText().Equals("Perfect Landing!"))
            pUI.TextFeedback("", -1); //empty feedback text
    }
    void ClearEarlyLandingTextFeedback()
    {
        if (!isLanding)
        {
            pUI.TextFeedback("", -1); //empty feedback text
            attemptedLand = false; //let player attempt landing again if still in air
        }
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

        #region DEVELOPER TOOLS
        if (Input.GetKeyDown(KeyCode.Semicolon))
        {
            dodgeAlways = !dodgeAlways;
            pUI.ToggleDodgeAlways();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            landAlways = !landAlways;
            pUI.ToggleLandAlways();
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            wcAlways = !wcAlways;
            pUI.ToggleWCAlways();
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            jAlways = !jAlways;
            pUI.ToggleJumpAlways();
        } 
        #endregion

        #region ANIMATION VARS
        isJumping = !AnimCheck(1, "Empty"); //check jumping anims
        isClimbing = AnimCheck(0, "Wall Climb") || AnimCheck(0, "WC Fail"); //check climbing anims
        isLanding = AnimCheck(0, "Land1") || AnimCheck(0, "Land2"); //check if landing
        isPunched = AnimCheck(3, "Punched"); //check if being punched by enemy
        defaultMove = !(isJumping || isClimbing || isLanding || AnimCheck(0, "Exit")); //false if player is any of these states

        if (AnimCheck(0, "Exit"))
            anim.SetBool("Exit", true);
        else if (anim.GetBool("Exit"))
            anim.SetBool("Exit", false);
        #endregion

        #region RAYCAST POSITIONS
        raypos[1] = transform.position + Vector3.up * distances[0] + -transform.right * distances[3];
        raypos[2] = transform.position + Vector3.up * distances[6];
        //Debug.DrawLine(raypos[2], raypos[2] + Vector3.back * distances[7], Color.cyan);
        //Debug.DrawLine(raypos[2] + Vector3.up * .1f, raypos[2] + Vector3.back * distances[8] + Vector3.up * .1f, Color.red);
        Vector3 visualOffset = new Vector3(0, 0, -.1f);
        //Debug.DrawLine(raypos[0] + visualOffset, raypos[0] + Vector3.down * distances[5] + visualOffset, Color.cyan);
        //Debug.DrawLine(raypos[0], raypos[0] + Vector3.down * distances[4], Color.red);
        //Debug.DrawRay(raypos[0], rayDir, Color.cyan, .1f); 
        #endregion

        #region LANDING, FALLING, AND GROUNDCHECK

        distances[4] = (-pm.fallvelocity.y) / dDominator; //distance for landing
        distances[5] = distances[4] + ldInputGap; //distance for landing input by player

        //raycast position for checking if player is grounded
        raypos[0] = transform.position + Vector3.up * distances[0] + -transform.right * distances[2]; 
        //check if player is in air (not grounded)
        if (!isLanding && !isClimbing && cc.enabled && !Physics.Raycast(raypos[0], Vector3.down, out hits[0], distances[0], groundMask)) 
        {
            isGrounded = false;

            //if in landing tutorial, check if ground is within distance to land to light up tutorial input text
            if (gm.tutNumber == 1 && Physics.Raycast(raypos[0], rayDir, out hits[0], distances[5], groundMask))
            {
                //print(Time.time); //figuring out the land input gap length (3 = 1/5 of a second)
                if (!gm.GetInputTextLit())
                    gm.LightUpInputText(true);//light up tutorial input text  for landing

                if (firstAction)
                {
                    firstAction = false;
                    this.CallDelay(gm.FreezeTutorial, .1f);
                }
            }

            if (Input.GetKeyDown(KeyCode.S) && !attemptedLand) //landing input
            {
                if (gm.tutNumber == 1 && !doneFirstAction)
                {
                    if (Time.timeScale == 0) //if frozen, unfreeze
                    {
                        doneFirstAction = true;
                        Time.timeScale = gm.timeScale / 100;
                    }
                    else
                        return;
                }

                if (Physics.Raycast(raypos[0], rayDir, out hits[0], distances[5], groundMask)) //check if ground is within distance to land
                {
                    anim.SetBool(hashFall, false); //land success
                    if (gm.tutNumber == 1) //if player is in tutorial for landing, increase counter
                        gm.IncreaseCounter();
                    pUI.TextFeedback("Perfect Landing!", 3);
                }
                else
                {
                    anim.SetBool(hashFall, !landAlways); //land fail unless set to always land
                    pUI.TextFeedback("Early Landing", 4);
                    this.CallDelay(ClearEarlyLandingTextFeedback, lResetDelay); //if too early then give second chance for landing
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
                        anim.SetBool(hashFall, !landAlways); //land fail unless set to always land
                        pUI.TextFeedback("Late Landing", 4);
                        //gm.DecreaseCounter(); //if player is in tutorial for jumping, decrease counter
                    }
                    anim.SetBool(hashLand, true);
                    this.CallDelay(ClearTextFeedback, lResetDelay);
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

            if (gm.tutNumber == 1 && gm.GetInputTextLit())
            {
                gm.LightUpInputText(false); //unlight the input text for landing
            }
        }
        anim.SetBool("IsGrounded", isGrounded); //correlate animation vars 
        #endregion

        #region WALL CLIMB FAIL & AUTO WALL CLIMB & AUTO JUMP
        if (defaultMove && !isClimbingFail && Physics.Raycast(raypos[2], Vector3.back, out hits[2], distances[8], wallMask)) //check if player is too close to wall
        {
            isClimbingFail = true;

            if (!attemptedClimb) //check if player didn't do anything or pressed key too late
                pUI.TextFeedback("Too Late To Climb", 4);
            attemptedClimb = true;

            //StartCoroutine(WallClimbDebug());
            wallClimbFailRoutine = StartCoroutine(WallClimbFail());
        }

        if (defaultMove && wcAlways && 
            Physics.Raycast(raypos[2], Vector3.back, out hits[2], distances[7], wallMask)) //auto wall climb
        {
            anim.SetBool(hashWallClimb, WallClimbCheck());
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

        #region TUTORIAL FOR WALL CLIMB & JUMP
        if (gm.tutNumber == 2)
        {
            if (defaultMove && Physics.Raycast(raypos[2], Vector3.back, out hits[2], distances[7], wallMask)) //for wall climbing
            {
                //print(Time.time); //figuring out the land input gap length (3 = 1/5 of a second)
                if (!gm.GetInputTextLit())
                    gm.LightUpInputText(true);//light up tutorial input text for wall climb

                if (firstAction)
                {
                    firstAction = false;
                    this.CallDelay(gm.FreezeTutorial, .1f);
                }
            }
            else if (gm.GetInputTextLit())
                gm.LightUpInputText(false);//light up tutorial input text for wall climb
        }
        #endregion

        #region PUNCHING & DODGING

        if (isPunched)
        {
            punchedLayerWeight += Time.deltaTime * punchedWeightSpeed; //transition to layer 3 (punched by enemy animation)

            if (!(punchedLayerWeight > 1)) //check if out of bounds
                anim.SetLayerWeight(3, punchedLayerWeight); //correlate vars
            else //if over 1, then equal 1
                anim.SetLayerWeight(3, 1); //correlate vars
        }
        else //not in punched animation
        {
            //CORRELATE VARS:
            punchedLayerWeight = 0;
            anim.SetLayerWeight(3, punchedLayerWeight);
        }

        if (dodgedEnemy && dodgeEnemyRoutine == null)
            dodgeEnemyRoutine = StartCoroutine(DodgeEnemy());


        if (Input.GetKeyDown(KeyCode.D) && !dodgeNow)
        {
            attemptDodge = true;
            if (dodgeMashPreventRoutine != null)
                StopCoroutine(dodgeMashPreventRoutine);
            dodgeMashPreventRoutine = StartCoroutine(ResetDodgeMashPrevent());
        }

        #endregion

    }

    #region PUNCHING & DODGING METHODS
    public IEnumerator PunchedByEnemy()
    {
        bool alreadyDodged = dodgedEnemy; //var for preventing player to dodge again
        ToggleCC_OFF(); //prevent enemy collision

        float timeToDodge = .266f; //start punched animation frame 10.5 (timeToDodge * 30 + 2.5)
        float initialTimeToDodge = timeToDodge;
        while (timeToDodge > 0)
        {
            timeToDodge -= Time.deltaTime;
            if(timeToDodge < initialTimeToDodge - dodgeDelay)
            {
                dodgeNow = true;
                //dodgedEnemy = true; /*always dodge early*/ print("Dodged Early");
            }

            if (Input.GetKeyDown(KeyCode.D) && dodgeNow && !alreadyDodged)
            {
                if (attemptDodge)
                    pUI.DontSpamUIToggle(); //tell player not to spam
                else
                {
                    dodgedEnemy = true;
                    pUI.TextFeedback("Dodged!", 3); //tell player that they dodged
                }
            }
            yield return null;
        }
        //dodgedEnemy = true; /*always dodge late*/ print("Dodged Late");
        if (dodgeAlways)
            dodgedEnemy = true;

        if (alreadyDodged)
        {
            dodgedEnemy = false; //prevent player from dodging again
            anim.SetFloat("FlipForPunch", 0); //flip punched animation
        }

        if (dodgedEnemy)
            ToggleCC_ON();
        else
        {
            if(gm.mode == 2)
            {
                if(attemptDodge)
                    pUI.TextFeedback("Dodged Too Early", 4); //tell player that they dodged too early
                else
                    pUI.TextFeedback("Dodged Too Late", 4); //tell player that they dodged too late
            }

            anim.SetBool(hashPunched, true); //start punched anaimation
            yield return new WaitForEndOfFrame();
            anim.SetBool(hashPunched, false); //prevent loop
        }

    }

    IEnumerator ResetDodgeMashPrevent()
    {
        yield return new WaitForSeconds(.5f);
        if (!dodgeNow)
            attemptDodge = false;
    }

    IEnumerator DodgeEnemy()
    {
        float weight = 0;
        while(weight < 1)
        {
            weight += Time.deltaTime * dodgeWeightSpeed; //transition to layer 4 (dodge enemy pose)
            anim.SetLayerWeight(4, weight); //correlate vars
            yield return null;
        }
        weight = 1;
        anim.SetLayerWeight(4, weight); //set layer weight to full influence
        while(weight > 0)
        {
            weight -= Time.deltaTime * unDodgeWeightSpeed; //transition to layer 4 (dodge enemy pose)
            anim.SetLayerWeight(4, weight); //correlate vars
            yield return null;
        }
        anim.SetLayerWeight(4, 0); //set layer weight to no influence
    }
    #endregion

    void DelayForAutoJump() => anim.SetBool("jumpDown", JumpDownCheck());

    //IEnumerator WallClimbDebug()
    //{
    //    float time_ = Time.time;
    //    while(!pUI.GetFeedbackText().Equals("Perfect Climb!"))
    //    {
    //        print("waiting for bug");
    //        yield return null;
    //    }
    //    print("TIME: " + (Time.time - time_));
    //}

    IEnumerator WallClimbFail()
    {
        //GameManager.time = Time.time;
        //print(transform.position.z);
        //yield return new WaitForSeconds(.1f);
        //print(transform.position.z);
        //print("WCF TIME:" + (Time.time - GameManager.time));
        yield return null;

        if (!pUI.GetFeedbackText().Equals("Perfect Climb!"))
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
        }
        else
            print("BUG FIXED");

        isClimbingFail = false;
    }

    public void StopWallClimbFailRoutine()
    {
        if(wallClimbFailRoutine != null)
            StopCoroutine(wallClimbFailRoutine);
    }
}
