using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    #region COMPONENTS
    PlayerMovement pm;
    PlayerTrick pt;
    Animator anim;
    GameManager gm;
    #endregion
    #region COLORS //m-Material
    [SerializeField]
    Material[] mColors;
    Color targetColor;
    [SerializeField]
    float sColorSpeed;
    bool sChangingColor = false;
    #endregion
    #region SLIDERS //s-Slider
    [SerializeField]
    Slider speedSlider;
    [SerializeField]
    Graphic speedFillSlider;
    [SerializeField] 
    float speed_sVelocity;
    float speed_sTarget;
    [SerializeField]
    float speed_sValue = 0;
    float speed_sDelta;
    [SerializeField]
    Slider fallSlider;
    [SerializeField]
    Graphic fallFillSlider;
    [SerializeField]
    float fall_sVelocity;
    float fall_sTarget;
    [SerializeField]
    float fall_sValue = 0;
    float fall_sDelta;
    #endregion
    #region FEEDBACK //f-Feedback
    [SerializeField]
    TextMeshProUGUI fText;
    [SerializeField]
    GameObject fParent;
    [SerializeField]
    float fTime;
    #endregion
    #region HASHES
    private int hashFall;
    private int hashWallClimb;
    private int hashClimbFail;
    private int hashClimbSpeed;
    private int hashLand;
    #endregion

    public bool startMethodCalled = false;
    void Start()
    {
        pm = gameObject.GetComponent<PlayerMovement>();
        pt = gameObject.GetComponent<PlayerTrick>();
        anim = gameObject.GetComponent<Animator>();
        gm = FindObjectOfType<GameManager>();

        hashFall = Animator.StringToHash("Fall");
        hashWallClimb = Animator.StringToHash("Climb");
        hashClimbFail = Animator.StringToHash("Climb Fail");
        hashClimbSpeed = Animator.StringToHash("Climb Speed");
        hashLand = Animator.StringToHash("Land");

        startMethodCalled = true;
    }

    void Update()
    {
        if (!pm.startMethodCalled || !pt.startMethodCalled)
            return;

        #region FORWARD SPEED SLIDER VALUE
        if (pm.velocityZ < 0) //Player's foward speed = target value
            speed_sTarget = 0;
        else if (pm.velocityZ > 12)
            speed_sTarget = 12;
        else
            speed_sTarget = pm.velocityZ;
        //MAKE THE VALUE GRADUALLY CHANGE TO TARGET:
        speed_sDelta = speed_sTarget - speed_sValue; //difference between target value and actual value (slider's value)
        speed_sDelta *= Time.deltaTime * speed_sVelocity; //make actual value gradually change
        speed_sValue += speed_sDelta; //increase actual value closer to target value
        speedSlider.value = speed_sValue; //correlate slider value 
        #endregion
        #region FORWARD SPEED SLIDER COLORS
        if (targetColor != mColors[2].color && pt.defaultMove) //not tricking
        {
            targetColor = mColors[2].color;
        }
        else if (targetColor != mColors[3].color &&
            ((pt.isLanding && !anim.GetBool(hashFall)) || (pt.isClimbing && !anim.GetBool(hashClimbFail)))) //trick success
        {
            targetColor = mColors[3].color;
        }
        else if (targetColor != mColors[4].color &&
            ((pt.isLanding && anim.GetBool(hashFall)) || (pt.isClimbing && anim.GetBool(hashClimbFail)))) //trick fail
        {
            targetColor = mColors[4].color;
        }
        if (!sChangingColor && speedFillSlider.color != targetColor)
            StartCoroutine(SpeedSliderColorChange(speedFillSlider.color, targetColor));
        #endregion
        #region FALL SPEED SLIDER VALUE
        if (-pm.fallvelocity.y < 2.1) //Player's fall speed = target value
            fall_sTarget = 2;
        else if (-pm.fallvelocity.y > 12)
            fall_sTarget = 12;
        else
            fall_sTarget = -pm.fallvelocity.y;
        if (fall_sTarget < fall_sValue) //if the value is decreasing then change value gradually
        {
            fall_sDelta = fall_sTarget - fall_sValue;
            fall_sDelta *= Time.deltaTime * fall_sVelocity;
            fall_sValue += fall_sDelta;
            fallSlider.value = fall_sValue; //correlate slider value 
        }
        else
        {
            fall_sValue = fall_sTarget; //prevent value skips
            fallSlider.value = fall_sTarget; //correlate slider value 
        }
        #endregion
    }

    public void TextFeedback (string message, int color) //text for giving feedback to the player
    {
        if(fText.color == mColors[0].color || GetFeedbackText().Equals("Game Over")) //if text is important, don't change text
            return;

        if (message != null)
        {
            fText.text = message;
            if (message.Equals("")) //if clearing text
                fParent.SetActive(false);
            else
                fParent.SetActive(true);
        }

        if (color != -1)
            fText.color = mColors[color].color;
    }
    public string GetFeedbackText() => fText.text;
    public void ClearImportantTextFeedback()
    {
        fText.text = "";
        TextFeedback("", -1);
    }

    IEnumerator SpeedSliderColorChange(Color startColor, Color endColor)
    {
        sChangingColor = true;
        float time = 0;
        while (time < 1f)
        {
            time += Time.deltaTime * sColorSpeed;
            speedFillSlider.color = Color.Lerp(startColor, endColor, time);
            yield return null;
        }
        speedFillSlider.color = endColor;
        sChangingColor = false;
    }
}