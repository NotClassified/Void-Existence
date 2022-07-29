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
    #region DEVELOPER UI
    public TextMeshProUGUI gpcText; //game progress count text
    #endregion

    private void Start()
    {
        //INITAILIZE THE MMCHILDREN ARRAY TO THE CHILDREN OF THIS TRANSFORM:
        mmChildren = new GameObject[transform.childCount];
        for(int i = 0; i < transform.childCount; i++)
        {
            mmChildren[i] = transform.GetChild(i).gameObject;
        }
        mmChildren[0].SetActive(false); //back
        mmChildren[1].SetActive(true);  //MM1
        mmChildren[2].SetActive(false);  //MM2

        UpdateGameProgress();
        //LOCKING AND UNLOCKING LEVELS:
        //for (int i = 0; i < levelButtons.Length && i < GameProgress.levelLastCompleted + 1; i++) //unlock levels completed plus 1 extra level
        //{
        //    levelButtons[i].GetComponent<Button>().interactable = true;
        //    levelButtons[i].transform.Find("#").gameObject.SetActive(true);
        //    levelButtons[i].transform.Find("Lock").gameObject.SetActive(false);
        //}
        //for (int i = GameProgress.levelLastCompleted + 1; i < levelButtons.Length; i++) //lock remaining levels
        //{
        //    levelButtons[i].GetComponent<Button>().interactable = false;
        //    levelButtons[i].transform.Find("#").gameObject.SetActive(false);
        //    levelButtons[i].transform.Find("Lock").gameObject.SetActive(true);
        //}
    }
    //call when button is pressed, find which button was pressed through parameter _button
    public void Button(string _button)
    {
        if(_button == "back")
        {
            mmChildren[0].SetActive(false);
            mmChildren[1].SetActive(true);
            mmChildren[2].SetActive(false);
        }
        else if (_button == "play")
        {
            mmChildren[0].SetActive(true);
            mmChildren[1].SetActive(false);
            mmChildren[2].SetActive(true);
        }
        else if (_button == "tutorial")
        {
            SceneManager.LoadScene(scenes[1]);
        }
        else if (_button == "quit")
        {
            Application.Quit();
        }
        else if (_button == "1")
        {
            SceneManager.LoadScene(scenes[2]);
        }
        //else if (_button == "")
        //{

        //}
    }

    void UpdateGameProgress()
    {
        if(GameProgress.tutorialLastCompleted >= 1)
        {
            levelButtons[1].GetComponent<Button>().interactable = true;
            levelButtons[1].transform.Find("#").gameObject.SetActive(true);
            levelButtons[1].transform.Find("Lock").gameObject.SetActive(false);
        }
        else
        {
            levelButtons[1].GetComponent<Button>().interactable = false;
            levelButtons[1].transform.Find("#").gameObject.SetActive(false);
            levelButtons[1].transform.Find("Lock").gameObject.SetActive(true);
        }
        gpcText.text = GameProgress.tutorialLastCompleted.ToString(); //set progression number
    }

    #region DEVELOPER UI
    public void GameProgressCountChange(int i)
    {
        if (!(GameProgress.tutorialLastCompleted == -1 && i < 0))
        {
            GameProgress.tutorialLastCompleted += i;
            UpdateGameProgress();
        }
    } 
    #endregion
}
