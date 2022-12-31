using UnityEngine;
using System.Collections;
using Cinemachine;

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
    public Transform rootBone;
    #endregion
    #region MOVEMENT VARS
    bool wallCLimbInput;
    bool jumpInput;
    public bool landInput;
    public bool dodgeInput;
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
    [SerializeField]
    GameObject camPrefab;
    [SerializeField]
    GameObject cinemachinePrefab;
    CinemachineVirtualCamera cinemachineVCam;
    [SerializeField]
    CinemachineBasicMultiChannelPerlin cinemachineNoise;
    [SerializeField]
    float camShakeMax;
    [SerializeField]
    float camShakeVelocity;
    //[SerializeField]
    //Transform cam1;
    //Vector3 camOffset;
    //Vector3 camTarget;
    //[SerializeField]
    //float camFollowSpeed;
    #endregion
    #region HASHES
    //private int hashSideJump;
    private int hashVelocityZ;
    //private int hashGapJump;
    private int hashJumpDown;
    private int hashWallClimb;
    private int hashAirToClimb;
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
    #region ANDROID
    public bool androidBuild;
    private Touch touch;
    private Vector2 touchStartPos, touchEndPos;
    private bool touchMoving;
    [SerializeField] float minSwipeDistance;
    #endregion

    public bool startMethodCalled = false;
    void Start()
    {
        pt = GetComponent<PlayerTrick>();
        pUI = GetComponent<PlayerUI>();
        gm = FindObjectOfType<GameManager>();

        Instantiate(camPrefab);
        cinemachineVCam = Instantiate(cinemachinePrefab).GetComponent<CinemachineVirtualCamera>();
        cinemachineVCam.Follow = rootBone;
        cinemachineNoise = cinemachineVCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        //hashSideJump = Animator.StringToHash("sideJump");
        hashVelocityZ = Animator.StringToHash("velocityZ");
        //hashGapJump = Animator.StringToHash("gapJump");
        hashJumpDown = Animator.StringToHash("jumpDown");
        hashWallClimb = Animator.StringToHash("Climb");
        hashAirToClimb = Animator.StringToHash("AirToClimb");
        hashIsGrounded = Animator.StringToHash("IsGrounded");
        hashInAir = Animator.StringToHash("InAir");
        //camOffset = cam1.transform.localPosition;
        //bspOffset = bsp.transform.localPosition;

        if (androidBuild)
            Debug.Log("Android Build");

        startMethodCalled = true;
    }



    void Update()
    {
        #region DEVELOPER TOOLS
        if (!pUI.startMethodCalled || !pt.startMethodCalled)
            return;

        if (Input.GetKey(KeyCode.F) && GameManager.developerMode)
            velocityZ = 0;
        if (Input.GetKeyDown(KeyCode.T) && GameManager.developerMode)
        {
            float slowTimeSet = 50 / 100f; //enter the desired slower timescale
            float fastTimeSet = 500 / 100f; //enter the desired faster timescale
            if (Input.GetKey(KeyCode.LeftShift))
            {
                if (Time.timeScale == fastTimeSet)
                    Time.timeScale = gm.timeScale / 100; //reset time
                else
                    Time.timeScale = fastTimeSet; //change time
            }
            else
            {
                if (Time.timeScale == slowTimeSet)
                    Time.timeScale = gm.timeScale / 100; //reset time
                else
                    Time.timeScale = slowTimeSet; //change time
            }
        } 
        #endregion
        #region MOVEMENT INPUT CONTROLS
        if (androidBuild)
        {
            //prevent these booleans from being true for more than one frame at a time
            wallCLimbInput = false;
            jumpInput = false;
            landInput = false;
            dodgeInput = false;

            if (Input.touchCount > 0)
            {
                touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    touchStartPos = touch.position;
                }
                else if (touch.phase == TouchPhase.Moved && !touchMoving 
                    && Mathf.Abs(touch.position.y - touchStartPos.y) > minSwipeDistance) //swipe has started
                {
                    touchMoving = true; //prevent thecode below from being executed more than once at a time

                    touchEndPos = touch.position;
                    float y = touchEndPos.y - touchStartPos.y; //distances of swipe on the y axis
                    
                    //get direction of swipe
                    if (y > 0) //user has swiped up
                    {
                        wallCLimbInput = true;
                        jumpInput = true;
                    }
                    else //user has swiped down
                    {
                        dodgeInput = true;
                        landInput = true;
                    }
                }
                else if (touch.phase == TouchPhase.Ended && touchMoving)
                {
                    touchMoving = false; //user has swiped
                }
            }
        }
        else //PC
        {
            wallCLimbInput = Input.GetKeyDown(KeyCode.W);
            jumpInput = Input.GetKeyDown(KeyCode.Space);
            landInput = Input.GetKeyDown(KeyCode.S);
            dodgeInput = Input.GetKeyDown(KeyCode.D);
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
        if (jumpInput && (gm.levelnum >= 3 || gm.tutNumber == 3) && !GameManager.levelFinished) //checking input for jump down
        {
            pt.JumpDownCheck();
        }
        #endregion
        #region WALL CLIMB
        //checking if climbing animation has ended
        if (pt.isClimbing && Time.time > pt.wcClipEnd && animator.GetBool(hashWallClimb) && !GameManager.levelFinished)
        {
            transform.position = rootBone.position; //sync player's position to character (root bone)
            animator.SetBool(hashWallClimb, false); //end wall climb animation
            animator.SetBool(hashAirToClimb, false); //toggle off air to climb parameter
            pt.attemptedClimb = false; //let player be able to climb again
            pUI.TextFeedback("", -1); //empty the climb feedback text
            pt.ToggleCC_ON(); //enable collider
        }
        //checking input for wall climb
        if (wallCLimbInput && !pt.isClimbing && velocityZ > 5.9f && (gm.levelnum >= 2 || gm.tutNumber == 2)) 
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
            Debug.LogError("player has fallen off map");
            gm.ReloadLevel();
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

    public IEnumerator CameraShake()
    {
        float delta;
        float subtractValue = 0;
        float value = camShakeMax;
        while (value > .1f)
        {
            delta = camShakeMax - subtractValue; //difference between target value and subtract value
            delta *= Time.deltaTime * camShakeVelocity; //make subtract value gradually change
            subtractValue += delta; //increase subtract value closer to target value

            cinemachineNoise.m_AmplitudeGain = value; //correlate values
            value = camShakeMax - subtractValue; //use subtract value so that actual value decreases
            yield return null;
        }
        cinemachineNoise.m_AmplitudeGain = 0;
    }

    public void ClipDuration() => print(animator.GetCurrentAnimatorStateInfo(0).length);

    public IEnumerator BoostPlayer(float boost, float duration, float boostDecay)
    {
        velocityZ = 12; //boost player forward
        float time = 1;
        Vector3 move = new Vector3(0, 0, -boost);
        bool audioPlayed = false;
        while (!pt.isJumping)
            yield return null;
        while (time < duration && pt.isJumping && cc.enabled)
        {
            time++;
            cc.Move(move);
            move = new Vector3(0, 0, move.z / boostDecay);

            if(!audioPlayed && time > pt.jAudioDelay)
            {
                audioPlayed = true;
                AudioManager.instance.PlaySound("jump");
            }
            yield return new WaitForFixedUpdate();
        }
    }

}
