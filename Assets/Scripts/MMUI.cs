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
    #region TESTING
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

        gpcText.text = GameProgress.levelCompleted.ToString(); //get progression number
        //LOCKING AND UNLOCKING LEVELS:
        for (int i = 0; i < levelButtons.Length && i < GameProgress.levelCompleted + 1; i++) //unlock levels completed plus 1 extra level
        {
            levelButtons[i].GetComponent<Button>().interactable = true;
            levelButtons[i].transform.Find("#").gameObject.SetActive(true);
            levelButtons[i].transform.Find("Lock").gameObject.SetActive(false);
        }
        for (int i = GameProgress.levelCompleted + 1; i < levelButtons.Length; i++) //lock remaining levels
        {
            levelButtons[i].GetComponent<Button>().interactable = false;
            levelButtons[i].transform.Find("#").gameObject.SetActive(false);
            levelButtons[i].transform.Find("Lock").gameObject.SetActive(true);
        }
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
        else if (_button == "1.1")
        {
            SceneManager.LoadScene(scenes[2]);
        }
        //else if (_button == "")
        //{

        //}
    }

    #region TESTING
    public void GameProgressCountChange(int i)
    {
        if (!(GameProgress.levelCompleted == -1 && i < 0))
        {
            GameProgress.levelCompleted += i;
            Start();
        }
    } 
    #endregion
}
