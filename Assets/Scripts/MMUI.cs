using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MMUI : MonoBehaviour
{
    private GameObject[] mmChildren;
    [SerializeField]
    string[] scenes;

    [SerializeField]
    GameObject[] levelButtons;

    [SerializeField] bool allowDevMode;

    private void Awake()
    {
        GameProgress.LoadGameProgress();
    }

    private void Start()
    {
        if (devProgressTool != null)
        {
            if (GameManager.developerMode)
                devProgressTool.SetActive(true);
            else
                devProgressTool.SetActive(false);
        }

        //INITAILIZE THE MMCHILDREN ARRAY TO THE CHILDREN OF THIS TRANSFORM:
        mmChildren = new GameObject[transform.childCount];
        for(int i = 1; i < transform.childCount; i++)
        {
            mmChildren[i-1] = transform.GetChild(i).gameObject;
        }
        if (GameProgress.tutorialLastCompleted > 0)
            MainMenuButtons("play");
        else
            MainMenuButtons("back");

        UpdateMenu();
    }

    //call when button is pressed, find which button was pressed through parameter _button
    public void MainMenuButtons(string _button)
    {
        switch (_button)
        {
            case "back":
                mmChildren[0].SetActive(false); //back
                mmChildren[1].SetActive(true); //main menu screen
                mmChildren[2].SetActive(false); //play menu screen
                break;
            case "play":
                mmChildren[0].SetActive(true); //back
                mmChildren[1].SetActive(false); //main menu screen
                mmChildren[2].SetActive(true); //play menu screen
                break;
            case "quit":
                Application.Quit();
                break;
            default: //load tutorial or level
                SceneManager.LoadScene(scenes[int.Parse(_button)]);
                break;

        }
    }

    void UpdateMenu()
    {
        for (int i = 0; i < levelButtons.Length; i++) 
        {
            //lock all levels
            levelButtons[i].GetComponent<Button>().interactable = false;
            levelButtons[i].transform.Find("#").gameObject.SetActive(false);
            levelButtons[i].transform.Find("Lock").gameObject.SetActive(true);

            //show best times for each level
            string timeFormatted = TimeObject.ConvertTimeMINSECMILI(GameProgress.levelTimeRecords[i + 1]);
            levelButtons[i].transform.Find("Time").GetComponent<TextMeshProUGUI>().text = timeFormatted;
        }
        for (int i = 0; i < levelButtons.Length && i < GameProgress.levelLastCompleted + 1; i++)
        {
            //unlock levels completed plus 1 extra level
            levelButtons[i].GetComponent<Button>().interactable = true;
            levelButtons[i].transform.Find("#").gameObject.SetActive(true);
            levelButtons[i].transform.Find("Lock").gameObject.SetActive(false);
        }
        gpcText.text = GameProgress.levelLastCompleted.ToString(); //set progression number

    }

    #region DEVELOPER UI
    public TextMeshProUGUI gpcText; //game progress count text
    [SerializeField] GameObject devProgressTool;

    public void GameProgressCountChange(int i)
    {
        if (!(GameProgress.levelLastCompleted == 0 && i < 0))
        {
            GameProgress.levelLastCompleted += i;
            GameProgress.tutorialLastCompleted += i;
            if (GameProgress.tutorialLastCompleted < 0)
                GameProgress.tutorialLastCompleted = 0;
            UpdateMenu();
        }
    }
    public void GameProgressReset()
    {
        GameProgress.ResetGameProgress();
        UpdateMenu();
    }
    private void Update()
    {
        #region DEVELOPER MODE
        if (allowDevMode
            && Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.D))
        {
            GameManager.developerMode = !GameManager.developerMode;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        #endregion
    }
    #endregion
}
