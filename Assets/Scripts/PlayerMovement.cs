using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    #region COMPONENTS
    [SerializeField]
    Animator animator;
    [SerializeField]
    CharacterController cc;
    PlayerTrick pt;
    PlayerUI pUI;
    public GameManager gm;
    [SerializeField]
    GameObject rootBone;
    #endregion
    #region MOVEMENT VARS
    public bool upInput;
    public float velocityZ;
    [SerializeField]
    float walkAcceleration;
    [SerializeField]
    float runAcceleration;
    [SerializeField]
    float maxWalkVelocity;
    [SerializeField]
    float maxRunVelocity;
    [SerializeField]
    float gravity;
    [SerializeField]
    float fowardDrag;
    public Vector3 fallvelocity;
    float inAirTransiPose;
    float inAirLayerWeight;
    [SerializeField]
    float inAirWeightSpeed;
    //bool trick;
    #endregion
    #region CAMERA CONTROLLERS
    private float camX;
    private float camY;
    [SerializeField]
    GameObject cam1;
    [SerializeField]
    GameObject cam2;
    [SerializeField]
    Transform head;
    private Vector3 camv3 = Vector3.zero;
    Vector3 wcOffset; //wc-Wall Climb
    [SerializeField]
    float mouseSens;
    [SerializeField]
    float controllerSens;
    #endregion
    #region HASHES
    //private int hashSideJump;
    private int hashVelocityZ;
    //private int hashGapJump;
    private int hashJumpDown;
    private int hashWallClimb;
    private int hashIsGrounded;
    private int hashInAir;
    //private string hashTrick = "trick"; 
    #endregion
    #region SHADOW //bsp-BlobShadowProjector
    [SerializeField]
    GameObject bsp;
    [SerializeField]
    GameObject bspOrtho;
    Vector3 bspOffset;
    #endregion
    public bool activeInputSystem = false;

    public bool startMethodCalled = false;
    void Start()
    {
        pt = GetComponent<PlayerTrick>();
        pUI = GetComponent<PlayerUI>();
        gm = FindObjectOfType<GameManager>();
        //hashSideJump = Animator.StringToHash("sideJump");
        hashVelocityZ = Animator.StringToHash("velocityZ");
        //hashGapJump = Animator.StringToHash("gapJump");
        hashJumpDown = Animator.StringToHash("jumpDown");
        hashWallClimb = Animator.StringToHash("Climb");
        hashIsGrounded = Animator.StringToHash("IsGrounded");
        hashInAir = Animator.StringToHash("InAir");
        wcOffset = cam1.transform.localPosition;
        bspOffset = bsp.transform.localPosition;

        startMethodCalled = true;
    }



    void Update()
    {
        if (!pUI.startMethodCalled || !pt.startMethodCalled)
            return;

        if (Input.GetKeyDown(KeyCode.Q))
            ResetPlayer(true);
        if (Input.GetKey(KeyCode.F))
            velocityZ = 0;
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (Time.timeScale == 0)
                Time.timeScale = gm.timeScale / 100; //reset time
            else
                Time.timeScale = .1f; //change time
        }
        //Cursor.lockState = CursorLockMode.Locked;
        #region MOVEMENT INPUT CONTROLS
        if (!activeInputSystem)
        {
            //levelAxis = Input.GetAxis("Horizontal");
            //forwardInput = true;//Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftControl);
            //runInput = true;//Input.GetKey(KeyCode.LeftShift);
            //leftInput = Input.GetKeyDown(KeyCode.A);
            //rightInput = Input.GetKeyDown(KeyCode.D);
            upInput = Input.GetKeyDown(KeyCode.W);
        }
        else
        {
            //forwardInput = forwardAxis > 0;
            //runInput = forwardAxis > .7;
        }
        #endregion
        #region FORWARD MOVEMENT
        if (velocityZ < 0) //prevent forward velocity from being negative
            velocityZ = 0;
        if (velocityZ < maxWalkVelocity && pt.isGrounded && !pt.isClimbing) //increase forward velocity when below walking speed and is grounded
            velocityZ += (walkAcceleration / 10) * Time.deltaTime;
        else if (velocityZ < maxRunVelocity && pt.isGrounded && !pt.isClimbing) //increse forward velocity when below running speed and is grounded
            velocityZ += (runAcceleration / 10) * Time.deltaTime;
        if (velocityZ != 9 && pt.AnimCheck(0, "Land1") || pt.isClimbing) //setting forward velocity when landing or climbing successfully
        {
            velocityZ = 9;
        }
        else if (velocityZ != 6 && pt.AnimCheck(0, "Land2")) //setting velocity when landing unsuccessfully
            velocityZ = 3;
        animator.SetFloat(hashVelocityZ, velocityZ); //correlating variables with animater
        #endregion
        #region JUMP DOWN
        if (animator.GetBool(hashJumpDown)) //prevent loop of the jump down animation
            animator.SetBool(hashJumpDown, false);
        if (upInput) //checking input for jump down
        {
            animator.SetBool(hashJumpDown, pt.JumpDownCheck());
        }
        //jumpInput = false; //prevent loop of the side jump animation 
        #endregion
        #region WALL CLIMB
        if ((pt.isClimbing && Time.time > pt.wcClipEnd && animator.GetBool(hashWallClimb))) //checking if climbing animation has ended
        {
            transform.position = rootBone.transform.position; //sync player's position to character (root bone)
            animator.SetBool(hashWallClimb, false); //end wall climb animation
            pt.attemptedClimb = false; //let player be able to climb again
            pUI.TextFeedback("", -1); //empty the climb feedback text
            //this.CallDelay(pt.ToggleCC, .2f);
            pt.ToggleCC_ON(); //enable collider
        }
        if (upInput && !pt.isClimbing && velocityZ > 5.9f) //checking input for wall climb
            pt.WallClimbCheck();
        #endregion
        #region FALLING
        if (pt.isGrounded && fallvelocity.y < 0)
            fallvelocity.y = -2f; //constant gravity to player to stick to ground
        else if (!pt.isClimbing)
            fallvelocity.y += -gravity * Time.deltaTime; //increasing downward force
        else
            fallvelocity.y = 0f;

        if (cc.enabled)
            cc.Move(fallvelocity * Time.deltaTime); //apply expontential downward force

        if (fallvelocity.y < -16) //check if player has fallen off map
        {
            ResetPlayer(true);
            gm.DecreaseCounter();
        }

        animator.SetBool(hashIsGrounded, pt.isGrounded);
        if (!pt.isGrounded || pt.isJumping)
        {
            inAirLayerWeight += Time.deltaTime * inAirWeightSpeed; //transition to layer 1 (in air poses)
            //Debug.Log(-cc.velocity.y / 20);
            inAirTransiPose = -cc.velocity.y / 20; //gradually transition from air pose 1 to air pose 2
            if (velocityZ > 0)
                velocityZ -= fowardDrag * Time.deltaTime; //decrease forward speed when in air
        }
        else if (pt.isGrounded || pt.isLanding)
        {
            //RESET TRANSITION VARS:
            inAirTransiPose = 0;
            inAirLayerWeight = 0; 
        }

        if (!(inAirTransiPose > 1)) //check if out of bounds
            animator.SetFloat(hashInAir, inAirTransiPose); //correlate vars
        else
            animator.SetFloat(hashInAir, 1f); //if "inAirTransi" is over 1 then set to 1
        if (!(inAirLayerWeight > 1)) //check if out of bounds
            animator.SetLayerWeight(1, inAirLayerWeight); //correlate vars
        else
            animator.SetLayerWeight(1, 1f); //if "inAirWeight" is over 1 then set to 1
        #endregion
    }

    public void ClipDuration()
    {
        print(animator.GetCurrentAnimatorStateInfo(0).length);
    }
    
    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.E)) //activate side view
        {
            cam1.SetActive(false);
            cam2.SetActive(true);
        }
        if (Input.GetKeyUp(KeyCode.E)) //deactivate side view
        {
            cam1.SetActive(true);
            cam2.SetActive(false);
        }
        if (pt.isClimbing)
        {
            cam1.transform.position = rootBone.transform.position; //camera follows player
            cam1.transform.localPosition += wcOffset;
            bsp.transform.position = rootBone.transform.position; //shadow follows player
            bsp.transform.localPosition += bspOffset;
            //bspOrtho.transform.localPosition = bsp.transform.localPosition; //shadow follows player
        }
        else if(cam1.transform.localPosition != wcOffset || bsp.transform.localPosition != bspOffset)
        {
            cam1.transform.localPosition = wcOffset;
            bsp.transform.localPosition = bspOffset;
        }
    }

    public IEnumerator BoostPlayer(float boost, float duration, float boostDecay)
    {
        velocityZ = 12; //boost player forward
        float time = 1;
        Vector3 move = new Vector3(0, 0, -boost);
        while (!pt.isJumping)
            yield return null;
        while (duration > time && pt.isJumping)
        {
            time++;
            cc.Move(move);
            move = new Vector3(0, 0, move.z / boostDecay);
            yield return new WaitForFixedUpdate();
        }
    }

    public void ResetPlayer(bool callGameManagerSpawnMethod)
    {
        if (pt.isClimbing)
        {
            transform.position = rootBone.transform.position; //sync player's position to character (root bone)
            animator.SetBool(hashWallClimb, false); //end wall climb animation
            pt.ToggleCC_ON(); //enable collider
        }
        StopCoroutine(gm.LastCountWaitForLand()); //if player doesn't jump far enough don't let player finish tutorial
        pt.StopWallClimbFailRoutine();

        //animator.SetLayerWeight(3, 0); //reset punched layer

        pt.attemptedClimb = false; //let player be able to climb again
        pt.attemptedLand = false; //let player be able to land again
        pUI.TextFeedback("", -1); //empty the climb feedback text
        fallvelocity.y = 0f;

        if (callGameManagerSpawnMethod)
            gm.StartSpawnRoutine(false); //reset player postition to starting point
        animator.Play("Exit", 0);
    }
}
