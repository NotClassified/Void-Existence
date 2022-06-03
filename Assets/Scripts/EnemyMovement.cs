using UnityEngine;
using System.Collections;

public class EnemyMovement : MonoBehaviour
{
    #region COMPONENTS
    [SerializeField]
    Animator animator;
    [SerializeField]
    CharacterController cc;
    [SerializeField]
    EnemyTrick et;
    public GameManager gm;
    [SerializeField]
    GameObject rootBone;
    #endregion
    #region MOVEMENT VARS
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

    private void Start()
    {
        gm = FindObjectOfType<GameManager>();
        //hashSideJump = Animator.StringToHash("sideJump");
        hashVelocityZ = Animator.StringToHash("velocityZ");
        //hashGapJump = Animator.StringToHash("gapJump");
        hashJumpDown = Animator.StringToHash("jumpDown");
        hashWallClimb = Animator.StringToHash("Climb");
        hashIsGrounded = Animator.StringToHash("IsGrounded");
        hashInAir = Animator.StringToHash("InAir");
        bspOffset = bsp.transform.localPosition;

        startMethodCalled = true;
    }



    void Update()
    {
        if (!et.startMethodCalled)
            return;

        if (Input.GetKey(KeyCode.F))
            velocityZ = 0;
        //Cursor.lockState = CursorLockMode.Locked;
        #region FORWARD MOVEMENT
        if (velocityZ < 0) //prevent forward velocity from being negative
            velocityZ = 0;
        if (velocityZ < maxWalkVelocity && et.isGrounded && !et.isClimbing) //increase forward velocity when below walking speed and is grounded
            velocityZ += (walkAcceleration / 10) * Time.deltaTime;
        else if (velocityZ < maxRunVelocity && et.isGrounded && !et.isClimbing) //increse forward velocity when below running speed and is grounded
            velocityZ += (runAcceleration / 10) * Time.deltaTime;
        if (velocityZ != 9 && et.AnimCheck(0, "Land1") || et.AnimCheck(0, "Wall Climb")) //setting forward velocity when landing or climbing successfully
            velocityZ = 9;
        else if (velocityZ != 6 && et.AnimCheck(0, "Land2")) //setting velocity when landing unsuccessfully
            velocityZ = 3;
        animator.SetFloat(hashVelocityZ, velocityZ); //correlating variables with animater
        #endregion
        #region JUMP DOWN
        if (animator.GetBool(hashJumpDown)) //prevent loop of the jump down animation
            animator.SetBool(hashJumpDown, false);
        #endregion
        #region WALL CLIMB
        if (et.isClimbing && Time.time > et.wcClipEnd && animator.GetBool(hashWallClimb)) //checking if climbing animation has ended
        {
            print("00");
            transform.position = rootBone.transform.position; //sync player's position to character (root bone)
            animator.SetBool(hashWallClimb, false); //end wall climb animation
            et.ToggleCC_ON(); //enable collider
        }
        #endregion
        #region FALLING
        if (et.isGrounded && fallvelocity.y < 0)
            fallvelocity.y = -2f; //constant gravity to player to stick to ground
        else if (!et.isClimbing)
            fallvelocity.y += -gravity * Time.deltaTime; //increasing downward force
        else
            fallvelocity.y = 0f;

        if (cc.enabled)
            cc.Move(fallvelocity * Time.deltaTime); //apply expontential downward force

        animator.SetBool(hashIsGrounded, et.isGrounded);
        if (!et.isGrounded || et.isJumping)
        {
            inAirLayerWeight += Time.deltaTime * inAirWeightSpeed; //transition to layer 1 (in air poses)
            //Debug.Log(-cc.velocity.y / 20);
            inAirTransiPose = -cc.velocity.y / 20; //gradually transition from air pose 1 to air pose 2
            if (velocityZ > 0)
                velocityZ -= fowardDrag * Time.deltaTime; //decrease forward speed when in air
        }
        else if (et.isGrounded || et.isLanding)
        {
            //reset transition vars:
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
        if (et.isClimbing)
        {
            bsp.transform.position = rootBone.transform.position;
            bsp.transform.localPosition += bspOffset;
            bspOrtho.transform.localPosition = bsp.transform.localPosition;
        }
        else if (bsp.transform.localPosition != bspOffset)
        {
            bsp.transform.localPosition = bspOffset;
        }
    }

    public IEnumerator BoostPlayer(float boost, float duration, float boostDecay)
    {
        float time = 1;
        Vector3 move = new Vector3(0, 0, -boost);
        while (!et.isJumping || !cc.enabled)
            yield return null;
        while (duration > time && et.isJumping)
        {
            time++;
            cc.Move(move);
            move = new Vector3(0, 0, move.z / boostDecay);
            yield return new WaitForFixedUpdate();
        }
    }

    public void ResetPlayer()
    {
        if (et.isClimbing)
        {
            transform.position = rootBone.transform.position; //sync player's position to character (root bone)
            animator.SetBool(hashWallClimb, false); //end wall climb animation
            et.ToggleCC_ON(); //enable collider
        }
        animator.Play("Exit", 0);
        fallvelocity.y = 0f;

        et.StopEnemyPunchRoutine();
        //animator.SetLayerWeight(2, 0);
    }
}
